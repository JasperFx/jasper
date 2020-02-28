using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Jasper.Util;

namespace Jasper.Runtime
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

        /// <summary>
        ///     Send to a specific destination rather than running the routing rules
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="destination">The destination to send to</param>
        /// <param name="message"></param>
        public Task SendToDestination<T>(Uri destination, T message)
        {
            var envelope = new Envelope {Message = message, Destination = destination};
            _root.Router.RouteToDestination(destination, envelope);

            trackEnvelopeCorrelation(envelope);

            return persistOrSend(envelope);
        }


        public async Task<Guid> SendEnvelope(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _root.Router.RouteOutgoingByEnvelope(envelope);

            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _root.MessageLogger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }

        /// <summary>
        /// Send a response message back to the original sender of the message being handled.
        /// This can only be used from within a message handler
        /// </summary>
        /// <param name="context"></param>
        /// <param name="response"></param>
        /// <returns></returns>
        public Task RespondToSender(object response)
        {
            if (Envelope == null) throw new InvalidOperationException("This operation can only be performed while in the middle of handling an incoming message");

            return SendToDestination(Envelope.ReplyUri, response);
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

                case Envelope env:
                    await SendEnvelope(env);
                    return;

                case IEnumerable<object> enumerable:
                    foreach (var o in enumerable) await EnqueueCascading(o);

                    return;

            }

            if (message.GetType().ToMessageTypeName() == Envelope.ReplyRequested)
            {
                await SendToDestination(Envelope.Destination, message);
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

                var outgoingEnvelopes = _root.Router.RouteOutgoingByEnvelope(envelope);

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

        public IMessageLogger Logger => _root.MessageLogger;

        public async Task SendAcknowledgement()
        {
            var ack = buildAcknowledgement();
            var outgoingEnvelopes = _root.Router.RouteOutgoingByMessage(ack);

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
                try
                {
                    await envelope.QuickSend();
                }
                catch (Exception e)
                {
                    Logger.LogException(e, envelope.CorrelationId, message:"Unable to send an outgoing message, most likely due to serialization issues");
                    Logger.DiscardedEnvelope(envelope);
                }
            }

            _outstanding.Clear();
        }

        public Task PublishEnvelope(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null)
                throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _root.Router.RouteOutgoingByEnvelope(envelope);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _root.MessageLogger.NoRoutesFor(envelope);
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

        public Task SendToTopic(object message, string topicName)
        {
            var envelope = new Envelope(message)
            {
                TopicName = topicName
            };

            var outgoing = _root.Router.RouteToTopic(topicName, envelope);
            return persistOrSend(outgoing);
        }

        public Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ExecutionTime = executionTime,
                Destination = TransportConstants.DurableLocalUri
            };

            var writer = _root.Serialization.JsonWriterFor(message.GetType());
            envelope.Data = writer.Write(message);
            envelope.ContentType = writer.ContentType;

            envelope.Status = EnvelopeStatus.Scheduled;
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
            envelope.Status = EnvelopeStatus.Scheduled;

            if (Persistence is NulloEnvelopePersistence)
            {
                _root.ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
                return Task.CompletedTask;
            }


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
            var outgoing = _root.Router.RouteOutgoingByMessage(message);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                throw new NoRoutesException(typeof(T));
            }

            return persistOrSend(outgoing);
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

        public async Task<T> Invoke<T>(object message)
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

            if (envelope.Response == null)
            {
                return default(T);
            }

            return (T)envelope.Response;
        }

        public Task Enqueue<T>(T message)
        {
            var envelope = _root.Router.RouteLocally(message);
            return persistOrSend(envelope);
        }

        public Task Enqueue<T>(T message, string workerQueue)
        {
            var envelope = _root.Router.RouteLocally(message, workerQueue);

            return persistOrSend(envelope);
        }

        private Task persistOrSend(Envelope envelope)
        {
            if (EnlistedInTransaction)
            {
                _outstanding.Add(envelope);
                return envelope.Sender.IsDurable ? Transaction.Persist(envelope) : Task.CompletedTask;
            }
            else
            {
                return envelope.Send();
            }
        }


        public Task Publish<T>(T message)
        {
            var envelope = new Envelope(message);
            return PublishEnvelope(envelope);
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


        private async Task persistOrSend(params Envelope[] outgoing)
        {
            if (EnlistedInTransaction)
            {
                await Transaction.Persist(outgoing.Where(isDurable).ToArray());

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



        private bool isDurable(Envelope envelope)
        {
            // SUPER HACK-y
            if (envelope.Callback is InvocationCallback) return false;

            return envelope.Sender?.IsDurable ?? _root.Runtime.GetOrBuildSendingAgent(envelope.Destination).IsDurable;
        }

        private void trackEnvelopeCorrelation(Envelope[] outgoing)
        {
            foreach (var outbound in outgoing)
            {
                trackEnvelopeCorrelation(outbound);
            }
        }

        private void trackEnvelopeCorrelation(Envelope outbound)
        {
            outbound.Source = _root.Settings.ServiceName;
            outbound.CorrelationId = CorrelationId;
            outbound.SagaId = _sagaId?.ToString() ?? Envelope?.SagaId ?? outbound.SagaId;

            if (Envelope != null) outbound.CausationId = Envelope.Id;
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


        /// <summary>
        ///     Send a message that should be executed at the given time
        /// </summary>
        /// <param name="message"></param>
        /// <param name="time"></param>
        /// <typeparam name="T"></typeparam>
        public Task ScheduleSend<T>(T message, DateTime time)
        {
            return SendEnvelope(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime(),
                Status = EnvelopeStatus.Scheduled
            });
        }

        /// <summary>
        ///     Send a message that should be executed after the given delay
        /// </summary>
        /// <param name="message"></param>
        /// <param name="delay"></param>
        /// <typeparam name="T"></typeparam>
        public Task ScheduleSend<T>(T message, TimeSpan delay)
        {
            return ScheduleSend(message, DateTime.UtcNow.Add(delay));
        }

    }
}
