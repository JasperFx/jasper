using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Conneg;
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
        private readonly IReplyWatcher _watcher;
        private readonly IHandlerPipeline _pipeline;
        private readonly SerializationGraph _serialization;
        private readonly MessagingSettings _settings;
        private readonly IChannelGraph _channels;
        private readonly IMessageLogger _logger;

        // TODO -- just pull in MessagingRoot?
        public MessageContext(IMessageRouter router, IReplyWatcher watcher, IHandlerPipeline pipeline, MessagingSerializationGraph serialization, MessagingSettings settings, IChannelGraph channels, IPersistence persistence, IMessageLogger logger)
        {
            _router = router;
            _watcher = watcher;
            _pipeline = pipeline;
            _serialization = serialization;
            _settings = settings;
            _channels = channels;
            Persistence = persistence;
            _logger = logger;
        }

        // TODO -- just pull in MessagingRoot?
        public MessageContext(IMessageRouter router, IReplyWatcher watcher, IHandlerPipeline pipeline, MessagingSerializationGraph serialization, MessagingSettings settings, IChannelGraph channels, IPersistence persistence, IMessageLogger logger, Envelope originalEnvelope)
        {
            _router = router;
            _watcher = watcher;
            _pipeline = pipeline;
            _serialization = serialization;
            _settings = settings;
            _channels = channels;
            Persistence = persistence;
            _logger = logger;

            Envelope = originalEnvelope;
            var persistor = new InMemoryEnvelopePersistor();
            EnlistedInTransaction = true;
            Persistor = persistor;

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
                ResponseId = Envelope.Id,
                Message = new Acknowledgement {CorrelationId = Envelope.Id},
                Route = new MessageRoute(typeof(Acknowledgement), Envelope.ReplyUri, "application/json")
                {
                    Channel = _channels.GetOrBuildChannel(Envelope.ReplyUri),

                },
                Writer = _serialization.JsonWriterFor(typeof(Acknowledgement))
            };

            return ack;
        }

        public IPersistence Persistence { get; }
        public Envelope Envelope { get; }

        private readonly List<Envelope> _outstanding = new List<Envelope>();

        public IEnumerable<Envelope> Outstanding => _outstanding;

        public bool EnlistedInTransaction { get; private set; }

        public Task EnlistInTransaction(IEnvelopePersistor persistor)
        {
            var original = Persistor;
            Persistor = persistor;
            EnlistedInTransaction = true;

            return original?.CopyTo(persistor) ?? Task.CompletedTask;
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
                await Persistor.Persist(outgoing.Where(x => x.Route.Channel.IsDurable));

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

        public async Task Publish(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));
            if (envelope.RequiresLocalReply) throw new ArgumentOutOfRangeException(nameof(envelope), "Cannot 'Publish' and envelope that requires a local reply");

            var outgoing = await _router.Route(envelope);
            trackEnvelopeCorrelation(outgoing);

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);
                return;
            }

            await persistOrSend(outgoing);
        }



        public async Task<Guid> SendEnvelope(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = await _router.Route(envelope);
            foreach (var env in outgoing)
            {
                _settings.ApplyMessageTypeSpecificRules(env);
            }

            trackEnvelopeCorrelation(outgoing);


            if (envelope.RequiresLocalReply)
            {
                if (outgoing.Length > 1)
                {
                    throw new InvalidOperationException("Cannot find a unique handler for this request");
                }

                // this is important for the request/reply mechanics
                outgoing[0].Id = envelope.Id;
            }

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            if (envelope.RequiresLocalReply)
            {
                foreach (var outgoingEnvelope in outgoing)
                {
                    await outgoingEnvelope.Send();
                }

                return envelope.Id;
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }

        private void trackEnvelopeCorrelation(Envelope[] outgoing)
        {
            if (Envelope != null)
            {
                foreach (var outbound in outgoing)
                {
                    outbound.OriginalId = Envelope.OriginalId;
                    outbound.ParentId = Envelope.Id;
                    outbound.SagaId = Envelope.SagaId;
                }
            }
        }

        public async Task<TResponse> Request<TResponse>(object request, TimeSpan timeout = default(TimeSpan),
            Action<Envelope> configure = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(request);
            configure?.Invoke(envelope);

            timeout = timeout == default(TimeSpan) ? 10.Seconds() : timeout;

            var watcher = _watcher.StartWatch<TResponse>(envelope.Id, timeout);

            await SendEnvelope(envelope);

            return await watcher;
        }

        public Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(message);
            envelope.ReplyUri = _channels.SystemReplyUri ?? envelope.ReplyUri;

            customization?.Invoke(envelope);

            return SendEnvelope(envelope);
        }

        public Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ExecutionTime = executionTime
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
                ? Persistor.ScheduleJob(envelope).ContinueWith(_ => envelope.Id)
                : Persistence.ScheduleJob(envelope).ContinueWith(_ => envelope.Id);
        }

        public Task<Guid> Schedule<T>(T message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }


        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageAlias();
            _serialization.RegisterType(typeof(TResponse));

            var reader = _serialization.ReaderFor(messageType);

            return new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes,
                RequiresLocalReply = true

            };
        }

        public Task Send<T>(T message)
        {
            return SendEnvelope(new Envelope {Message = message});
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
                ReplyRequested = typeof(T).ToMessageAlias(),
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

        public Task SendAndWait<T>(T message)
        {
            return GetSendAndWaitTask(message);
        }

        public Task SendAndWait<T>(Uri destination, T message)
        {
            return GetSendAndWaitTask(message, destination);
        }

        private async Task GetSendAndWaitTask<T>(T message, Uri destination = null)
        {
            var envelope = new Envelope
            {
                Message = message,
                AckRequested = true,
                Destination = destination,
                RequiresLocalReply = true
            };

            var task = _watcher.StartWatch<Acknowledgement>(envelope.Id, 10.Minutes());


            await SendEnvelope(envelope);

            await task;
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

        public IEnvelopePersistor Persistor { get; private set; }




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

            if (message.GetType().ToMessageAlias() == Envelope.ReplyRequested)
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
                    ResponseId = Envelope.Id,
                    Message = new FailureAcknowledgement()
                    {
                        CorrelationId = Envelope.Id,
                        Message = message
                    }
                };

                var outgoingEnvelopes = await _router.Route(envelope);

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
            var outgoingEnvelopes = await _router.Route(ack);

            foreach (var outgoing in outgoingEnvelopes)
            {
                await outgoing.Send();
            }
        }

        public IAdvancedMessagingActions Advanced => this;
    }



}
