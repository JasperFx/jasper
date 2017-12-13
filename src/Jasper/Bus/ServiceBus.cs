using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.Extensions.Logging;

namespace Jasper.Bus
{
    public interface IEnvelopePersistor
    {
        Task Persist(Envelope envelope);
        Task Persist(IEnumerable<Envelope> envelopes);
    }

    public partial class ServiceBus : IServiceBus
    {
        private readonly IMessageRouter _router;
        private readonly IReplyWatcher _watcher;
        private readonly IHandlerPipeline _pipeline;
        private readonly SerializationGraph _serialization;
        private readonly BusSettings _settings;
        private readonly IChannelGraph _channels;
        private readonly CompositeLogger _logger;

        public ServiceBus(IMessageRouter router, IReplyWatcher watcher, IHandlerPipeline pipeline, BusMessageSerializationGraph serialization, BusSettings settings, IChannelGraph channels, IPersistence persistence, CompositeLogger logger)
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

        public IPersistence Persistence { get; }
        public int UniqueNodeId => _settings.UniqueNodeId;

        private readonly List<Envelope> _outstanding = new List<Envelope>();
        private IEnvelopePersistor _persistor;

        public IEnumerable<Envelope> Outstanding => _outstanding;

        public bool EnlistedInTransaction { get; private set; }

        public void EnlistInTransaction(IEnvelopePersistor persistor)
        {
            _persistor = persistor;
            EnlistedInTransaction = true;
        }

        public async Task FlushOutstanding()
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
                await _persistor.Persist(outgoing.Where(x => x.Route.Channel.IsDurable));

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

            var outgoing = await _router.Route(envelope);

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);
                return;
            }

            await persistOrSend(outgoing);
        }



        public async Task<Guid> Send(Envelope envelope)
        {
            if (envelope.Message == null) throw new ArgumentNullException(nameof(envelope.Message));

            var outgoing = await _router.Route(envelope);

            if (!outgoing.Any())
            {
                _logger.NoRoutesFor(envelope);

                throw new NoRoutesException(envelope);
            }

            await persistOrSend(outgoing);

            return envelope.Id;
        }

        public async Task<TResponse> Request<TResponse>(object request, TimeSpan timeout = default(TimeSpan),
            Action<Envelope> configure = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(request);
            configure?.Invoke(envelope);

            timeout = timeout == default(TimeSpan) ? 10.Seconds() : timeout;

            var watcher = _watcher.StartWatch<TResponse>(envelope.Id, timeout);

            await Send(envelope);

            return await watcher;
        }

        public Task SendAndExpectResponseFor<TResponse>(object message, Action<Envelope> customization = null)
        {
            var envelope = EnvelopeForRequestResponse<TResponse>(message);
            envelope.ReplyUri = _channels.SystemReplyUri ?? envelope.ReplyUri;

            customization?.Invoke(envelope);

            return Send(envelope);
        }

        public Task<Guid> Schedule<T>(T message, DateTimeOffset executionTime)
        {
            var envelope = new Envelope(message)
            {
                ExecutionTime = executionTime
            };

            return Persistence.ScheduleMessage(envelope).ContinueWith(_ => envelope.Id);
        }

        public Task<Guid> Schedule<T>(T message, TimeSpan delay)
        {
            return Schedule(message, DateTimeOffset.UtcNow.Add(delay));
        }


        public Envelope EnvelopeForRequestResponse<TResponse>(object request)
        {
            var messageType = typeof(TResponse).ToMessageAlias();
            var reader = _serialization.ReaderFor(messageType);

            var envelope = new Envelope
            {
                Message = request,
                ReplyRequested = messageType,
                AcceptedContentTypes = reader.ContentTypes,
                RequiresLocalReply = true

            };
            return envelope;
        }

        public Task Send<T>(T message)
        {
            return Send(new Envelope {Message = message});
        }

        public Task Send<T>(T message, Action<Envelope> customize)
        {
            var envelope = new Envelope {Message = message};
            customize(envelope);

            return Send(envelope);
        }

        public Task Send<T>(Uri destination, T message)
        {
            return Send(new Envelope { Message = message, Destination = destination});
        }

        public Task Invoke<T>(T message)
        {
            return _pipeline.InvokeNow(new Envelope(message)
            {
                Callback = new InvocationCallback(),
                ReplyUri = TransportConstants.RepliesUri
            });
        }

        public Task Enqueue<T>(T message)
        {
            var isDurable = _settings.Workers.ShouldBeDurable(typeof(T));
            var uri = isDurable ? TransportConstants.DurableLoopbackUri : TransportConstants.LoopbackUri;

            var channel = _channels.GetOrBuildChannel(uri);


            var envelope = new Envelope(message);
            return channel.Send(envelope);
        }

        public Task DelaySend<T>(T message, DateTime time)
        {
            return Send(new Envelope
            {
                Message = message,
                ExecutionTime = time.ToUniversalTime()
            });
        }

        public Task DelaySend<T>(T message, TimeSpan delay)
        {
            return DelaySend(message, DateTime.UtcNow.Add(delay));
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


            await Send(envelope);

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


    }
}
