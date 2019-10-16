using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Util;

namespace Jasper.Messaging
{
    public class MessageContext : IMessageContext, IAdvancedMessagingActions
    {
        private readonly List<Envelope> _outstanding = new List<Envelope>();
        private readonly IMessagingRoot _root;
        private object _sagaId;

        public MessageContext(IMessagingRoot root)
        {
            _root = root;

            Persistence = root.Persistence;
        }

        public MessageContext(IMessagingRoot root, Envelope originalEnvelope) : this(root)
        {
            Envelope = originalEnvelope;
            _sagaId = originalEnvelope.SagaId;

            CorrelationId = originalEnvelope.CorrelationId;

            var persistor = new InMemoryEnvelopeTransaction();
            EnlistedInTransaction = true;
            Transaction = persistor;

            if (Envelope.AckRequested)
            {
                var ack = buildAcknowledgement();

                persistor.Queued.Fill(ack);
                _outstanding.Add(ack);
            }
        }

        public IEnvelopePersistence Persistence { get; }

        public IEnumerable<Envelope> Outstanding => _outstanding;

        public IEnvelopeTransaction Transaction { get; private set; }


        public async Task<Guid> SendEnvelope(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _root.Router.Route(envelope);
            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                for (int i = 0; i < outgoing.Length; i++)
                {
                    _root.ApplyMessageTypeSpecificRules(outgoing[i]);

                    var subscriber = _root.Subscribers.GetOrBuild(outgoing[i].Destination);

                    if (!subscriber.SupportsNativeScheduledSend)
                    {
                        outgoing[i] = outgoing[i].ForScheduledSend(subscriber);
                    }
                }
            }
            else
            {
                foreach (var env in outgoing) _root.ApplyMessageTypeSpecificRules(env);
            }




            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _root.Logger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }


        public async Task EnqueueCascading(object message)
        {
            if (Envelope.ResponseType != null && message?.GetType() == Envelope.ResponseType)
            {
                Envelope.Response = message;
                return;
            }

            switch (message)
            {
                case null:
                    return;


                case IEnumerable<object> enumerable:
                    foreach (var o in enumerable) await EnqueueCascading(o);

                    return;

                case ISendMyself sender:
                    var envelope = sender.CreateEnvelope(Envelope);
                    await SendEnvelope(envelope);
                    return;
            }

            if (message.GetType().ToMessageTypeName() == Envelope.ReplyRequested)
            {
                var envelope = Envelope.ForResponse(message);
                await SendEnvelope(envelope);
                return;
            }


            await Publish(message);
        }

        public async Task SendFailureAcknowledgement(string message)
        {
            if (Envelope.AckRequested || Envelope.ReplyRequested.IsNotEmpty())
            {
                var envelope = new Envelope
                {
                    CausationId = Envelope.Id,
                    Destination = Envelope.ReplyUri,
                    Message = new FailureAcknowledgement
                    {
                        CorrelationId = Envelope.Id,
                        Message = message
                    }
                };

                var outgoingEnvelopes = _root.Router.Route(envelope);

                foreach (var outgoing in outgoingEnvelopes)
                {
                    await outgoing.Send();
                }
            }
        }

        public Task Retry()
        {
            _outstanding.Clear();

            return _root.Pipeline.Invoke(Envelope);
        }

        public IMessageLogger Logger => _root.Logger;

        public async Task SendAcknowledgement()
        {
            var ack = buildAcknowledgement();
            var outgoingEnvelopes = _root.Router.Route(ack);

            foreach (var outgoing in outgoingEnvelopes)
            {
                await outgoing.Send();
            }
        }

        public Guid CorrelationId { get; } = CombGuidIdGeneration.NewGuid();
        public Envelope Envelope { get; }

        public bool EnlistedInTransaction { get; private set; }

        public Task EnlistInTransaction(IEnvelopeTransaction transaction)
        {
            var original = Transaction;
            Transaction = transaction;
            EnlistedInTransaction = true;

            return original?.CopyTo(transaction) ?? Task.CompletedTask;
        }

        public async Task SendAllQueuedOutgoingMessages()
        {
            foreach (var envelope in Outstanding)
            {
                await envelope.QuickSend();
            }

            _outstanding.Clear();
        }

        public Task Publish(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _root.Router.Route(envelope);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _root.Logger.NoRoutesFor(envelope);
                return Task.CompletedTask;
            }

            return persistOrSend(outgoing);
        }

        public Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(message);

            customization?.Invoke(envelope);

            return SendEnvelope(envelope);
        }

        public Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ExecutionTime = executionTime,
                Destination = TransportConstants.DurableLoopbackUri
            };

            if (envelope.Data == null || envelope.Data.Length == 0)
            {
                var writer = _root.Serialization.JsonWriterFor(message.GetType());
                envelope.Data = writer.Write(message);
                envelope.ContentType = writer.ContentType;
            }

            envelope.Status = TransportConstants.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            return ScheduleEnvelope(envelope).ContinueWith(_ => envelope.Id);
        }

        internal Task ScheduleEnvelope(Envelope envelope)
        {
            if (envelope.Message == null)
                throw new ArgumentOutOfRangeException(nameof(envelope), "Envelope.Message is required");

            if (!envelope.ExecutionTime.HasValue)
                throw new ArgumentOutOfRangeException(nameof(envelope), "No value for ExecutionTime");


            envelope.OwnerId = TransportConstants.AnyNode;
            envelope.Status = TransportConstants.Scheduled;


            return EnlistedInTransaction
                ? Transaction.ScheduleJob(envelope)
                : Persistence.ScheduleJob(envelope);
        }

        public Task<Guid> Schedule<T>(T message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }

        public Task Send<T>(T message)
        {
            var envelope = message as Envelope ?? new Envelope {Message = message};
            return SendEnvelope(envelope);
        }

        public Task Send<T>(T message, Action<Envelope> customize)
        {
            var envelope = new Envelope {Message = message};
            customize(envelope);

            return SendEnvelope(envelope);
        }

        public Task Send<T>(Uri destination, T message)
        {
            return SendEnvelope(new Envelope {Message = message, Destination = destination});
        }

        public Task Invoke(object message)
        {
            return _root.Pipeline.InvokeNow(new Envelope(message)
            {
                Callback = new InvocationCallback(),
                ReplyUri = TransportConstants.RepliesUri,
                CorrelationId = CorrelationId
            });
        }

        public async Task<T> Invoke<T>(object message) where T : class
        {
            if (message == null) throw new ArgumentNullException(nameof(message));

            var envelope = new Envelope(message)
            {
                Callback = new InvocationCallback(),
                ReplyUri = TransportConstants.RepliesUri,
                ReplyRequested = typeof(T).ToMessageTypeName(),
                ResponseType = typeof(T),
                CorrelationId = CorrelationId
            };

            await _root.Pipeline.InvokeNow(envelope);

            return envelope.Response as T;
        }

        public Task Enqueue<T>(T message, string workerQueue = null)
        {
            var isDurable = _root.ShouldBeDurable(typeof(T));
            var destination = isDurable ? TransportConstants.DurableLoopbackUri : TransportConstants.LoopbackUri;

            var envelope = new Envelope
            {
                Message = message,
                Destination = destination.AtQueue(workerQueue),
            };

            return SendEnvelope(envelope);
        }

        public Task EnqueueLightweight<T>(T message, string workerQueue = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                Destination = TransportConstants.LoopbackUri.AtQueue(workerQueue),
            };

            return SendEnvelope(envelope);
        }

        public Task EnqueueDurably<T>(T message, string workerQueue = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                Destination = TransportConstants.DurableLoopbackUri.AtQueue(workerQueue),
            };

            return SendEnvelope(envelope);
        }

        public Task ScheduleSend<T>(T message, DateTime time)
        {
            return SendEnvelope(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime(),
                Status = TransportConstants.Scheduled
            });
        }

        public Task ScheduleSend<T>(T message, TimeSpan delay)
        {
            return ScheduleSend(message, DateTime.UtcNow.Add(delay));
        }

        public Task Publish<T>(T message)
        {
            var envelope = new Envelope(message);
            return Publish(envelope);
        }

        public Task Publish<T>(T message, Action<Envelope> customize)
        {
            var envelope = new Envelope(message);
            customize(envelope);
            return Publish(envelope);
        }

        public IAdvancedMessagingActions Advanced => this;

        public void EnlistInSaga(object sagaId)
        {
            _sagaId = sagaId ?? throw new ArgumentNullException(nameof(sagaId));
            foreach (var envelope in _outstanding) envelope.SagaId = sagaId.ToString();
        }


        private Envelope buildAcknowledgement()
        {
            var writer = _root.Serialization.JsonWriterFor(typeof(Acknowledgement));
            var ack = new Envelope(new Acknowledgement {CorrelationId = Envelope.Id}, writer)
            {
                CausationId = Envelope.Id,
                Destination = Envelope.ReplyUri,
                SagaId = Envelope.SagaId,
            };

            return ack;
        }


        private async Task persistOrSend(Envelope[] outgoing)
        {
            if (EnlistedInTransaction)
            {
                await Transaction.Persist(outgoing.Where(x => _root.Subscribers.GetOrBuild(x.Destination).IsDurable).ToArray());

                _outstanding.AddRange(outgoing);
            }
            else
            {
                foreach (var outgoingEnvelope in outgoing)
                {
                    await outgoingEnvelope.Send();
                }
            }
        }

        private void trackEnvelopeCorrelation(Envelope[] outgoing)
        {
            foreach (var outbound in outgoing)
            {
                outbound.CorrelationId = CorrelationId;
                outbound.SagaId = _sagaId?.ToString() ?? Envelope?.SagaId ?? outbound.SagaId;
            }

            if (Envelope == null) return;

            foreach (var outbound in outgoing)
            {
                outbound.CausationId = Envelope.Id;
            }
        }


        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageTypeName();
            _root.Serialization.RegisterType(typeof(TResponse));

            var reader = _root.Serialization.ReaderFor(messageType);

            return new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes
            };
        }
    }
}
