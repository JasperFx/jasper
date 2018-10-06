using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Jasper.Messaging
{
    public class MessageContext : IMessageContext, IAdvancedMessagingActions
    {
        private readonly IMessageRouter _router;
        private readonly IHandlerPipeline _pipeline;
        private readonly SerializationGraph _serialization;
        private readonly MessagingSettings _settings;
        private readonly ISubscriberGraph _subscribers;
        private readonly IMessageLogger _logger;

        // TODO -- just pull in MessagingRoot?
        public MessageContext(IMessageRouter router, IHandlerPipeline pipeline,
            MessagingSerializationGraph serialization, MessagingSettings settings, ISubscriberGraph subscribers,
            IDurableMessagingFactory factory, IMessageLogger logger)
        {
            _router = router;
            _pipeline = pipeline;
            _serialization = serialization;
            _settings = settings;
            _subscribers = subscribers;
            Factory = factory;
            _logger = logger;
        }

        // TODO -- just pull in MessagingRoot?
        public MessageContext(IMessageRouter router, IHandlerPipeline pipeline,
            MessagingSerializationGraph serialization, MessagingSettings settings, ISubscriberGraph subscribers,
            IDurableMessagingFactory factory, IMessageLogger logger, Envelope originalEnvelope)
        {
            _router = router;
            _pipeline = pipeline;
            _serialization = serialization;
            _settings = settings;
            _subscribers = subscribers;
            Factory = factory;
            _logger = logger;

            Envelope = originalEnvelope;
            _sagaId = originalEnvelope.SagaId;


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

        private Envelope buildAcknowledgement()
        {
            var ack = new Envelope
            {
                ParentId = Envelope.Id,
                Destination = Envelope.ReplyUri,
                SagaId = Envelope.SagaId,
                Message = new Acknowledgement {CorrelationId = Envelope.Id},
                Route = new MessageRoute(typeof(Acknowledgement), Envelope.ReplyUri, "application/json")
                {
                    Channel = _subscribers.GetOrBuild(Envelope.ReplyUri),

                },
                Writer = _serialization.JsonWriterFor(typeof(Acknowledgement))
            };

            return ack;
        }

        public IDurableMessagingFactory Factory { get; }
        public Envelope Envelope { get; }

        private readonly List<Envelope> _outstanding = new List<Envelope>();
        private object _sagaId;

        public IEnumerable<Envelope> Outstanding => _outstanding;

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

        private async Task persistOrSend(Envelope[] outgoing)
        {
            if (EnlistedInTransaction)
            {
                await Transaction.Persist(outgoing.Where(x => x.Route.Channel.IsDurable).ToArray());

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

        public Task Publish(Envelope envelope)
        {
            if (envelope.Message == null && envelope.Data == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _router.Route(envelope);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);
                return Task.CompletedTask;
            }

            return persistOrSend(outgoing);
        }



        public async Task<Guid> SendEnvelope(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = _router.Route(envelope);
            foreach (var env in outgoing)
            {
                _settings.ApplyMessageTypeSpecificRules(env);
            }

            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }

        private void trackEnvelopeCorrelation(Envelope[] outgoing)
        {
            var now = DateTimeOffset.UtcNow;

            foreach (var envelope in outgoing)
            {
                envelope.SagaId = _sagaId?.ToString() ?? Envelope?.SagaId ?? envelope.SagaId;
            }

            if (Envelope == null) return;

            foreach (var outbound in outgoing)
            {
                outbound.OriginalId = Envelope.OriginalId;
                outbound.ParentId = Envelope.Id;
            }
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
                var writer = _serialization.JsonWriterFor(message.GetType());
                envelope.Data = writer.Write(message);
                envelope.ContentType = writer.ContentType;
            }

            envelope.Status = TransportConstants.Scheduled;
            envelope.OwnerId = TransportConstants.AnyNode;

            return EnlistedInTransaction
                ? Transaction.ScheduleJob(envelope).ContinueWith(_ => envelope.Id)
                : Factory.ScheduleJob(envelope).ContinueWith(_ => envelope.Id);
        }

        public Task<Guid> Schedule<T>(T message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }


        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageTypeName();
            _serialization.RegisterType(typeof(TResponse));

            var reader = _serialization.ReaderFor(messageType);

            return new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes

            };
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
            return SendEnvelope(new Envelope { Message = message, Destination = destination});
        }

        public Task Invoke(object message)
        {
            return _pipeline.InvokeNow(new Envelope(message)
            {
                Callback = new InvocationCallback(),
                ReplyUri = TransportConstants.RepliesUri
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
                ResponseType = typeof(T)

            };

            await _pipeline.InvokeNow(envelope);

            return envelope.Response as T;
        }

        public Task Enqueue<T>(T message, string workerQueue = null)
        {
            var isDurable = _settings.Workers.ShouldBeDurable(typeof(T));
            var destination = isDurable ? TransportConstants.DurableLoopbackUri : TransportConstants.LoopbackUri;

            var envelope = new Envelope
            {
                Message = message,
                Destination = destination,
                Queue = workerQueue
            };

            return SendEnvelope(envelope);
        }

        public Task EnqueueLightweight<T>(T message, string workerQueue = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                Destination = TransportConstants.LoopbackUri,
                Queue = workerQueue

            };

            return SendEnvelope(envelope);
        }

        public Task EnqueueDurably<T>(T message, string workerQueue = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                Destination = TransportConstants.DurableLoopbackUri,
                Queue = workerQueue
            };

            return SendEnvelope(envelope);
        }

        public Task ScheduleSend<T>(T message, DateTime time)
        {
            return SendEnvelope(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
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

        public IEnvelopeTransaction Transaction { get; private set; }




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
                    foreach (var o in enumerable)
                    {
                        await EnqueueCascading(o);
                    }

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
                    ParentId = Envelope.Id,
                    Destination = Envelope.ReplyUri,
                    Message = new FailureAcknowledgement()
                    {
                        CorrelationId = Envelope.Id,
                        Message = message
                    }
                };

                var outgoingEnvelopes = _router.Route(envelope);

                foreach (var outgoing in outgoingEnvelopes)
                {
                    await outgoing.Send();
                }
            }


        }

        public Task Retry()
        {
            _outstanding.Clear();

            return _pipeline.Invoke(Envelope);
        }

        public IMessageLogger Logger => _logger;

        public async Task SendAcknowledgement()
        {
            var ack = buildAcknowledgement();
            var outgoingEnvelopes = _router.Route(ack);

            foreach (var outgoing in outgoingEnvelopes)
            {
                await outgoing.Send();
            }
        }

        public IAdvancedMessagingActions Advanced => this;
        public void EnlistInSaga(object sagaId)
        {
            _sagaId = sagaId ?? throw new ArgumentNullException(nameof(sagaId));
            foreach (var envelope in _outstanding)
            {
                envelope.SagaId = sagaId.ToString();
            }
        }
    }



}
