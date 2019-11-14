using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging
{
    public class MessagingRoot : IDisposable, IMessagingRoot, IHostedService, ISubscriberGraph
    {
        private readonly IList<IListeningWorkerQueue> _listeners = new List<IListeningWorkerQueue>();

        private readonly object _channelLock = new object();

        private readonly Dictionary<string, ITransport> _transports = new Dictionary<string, ITransport>();

        private ImHashMap<Uri, ISubscriber> _subscribers = ImHashMap<Uri, ISubscriber>.Empty;

        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;


        private readonly Lazy<IEnvelopePersistence> _persistence;
        private readonly IContainer _container;

        public MessagingRoot(MessagingSerializationGraph serialization,
            JasperOptions options,
            IMessageLogger messageLogger,
            IContainer container,
            ITransportLogger transportLogger
            )
        {
            Options = options;
            Handlers = options.HandlerGraph;
            TransportLogger = transportLogger;
            Transports = container.QuickBuildAll<ITransport>().ToArray();

            Settings = options.Advanced;
            Serialization = serialization;

            Logger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, Handlers, Logger,
                container.QuickBuildAll<IMissingHandler>(),
                this);

            Router = new MessageRouter(Handlers, serialization, Options.Advanced, this);

            _persistence = new Lazy<IEnvelopePersistence>(() => container.GetInstance<IEnvelopePersistence>());

            _container = container;
        }

        public void Dispose()
        {
            foreach (var listener in _listeners) listener.SafeDispose();

            _listeners.Clear();

            foreach (var channel in _subscribers.Enumerate()) channel.Value.Dispose();


            _subscribers = ImHashMap<Uri, ISubscriber>.Empty;

            ScheduledJobs.Dispose();
        }

        public AdvancedSettings Settings { get; }

        public ITransportLogger TransportLogger { get; }

        public DurabilityAgent Durability { get; private set; }


        public ITransport[] Transports { get; }

        public IScheduledJobProcessor ScheduledJobs
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public JasperOptions Options { get; }

        public IMessageRouter Router { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger Logger { get; }

        public MessagingSerializationGraph Serialization { get; }

        public IEnvelopePersistence Persistence => _persistence.Value;

        public IMessageContext NewContext()
        {
            return new MessageContext(this);
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return new MessageContext(this, envelope);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await bootstrap();
            }
            catch (Exception e)
            {
                Logger.LogException(e, message:"Failed to start the Jasper messaging");
                throw;
            }
        }

        private async Task bootstrap()
        {
            await Handlers.Compiling;

            Handlers.Compile(Options.CodeGeneration, _container);

            if (Options.CodeGeneration.TypeLoadMode == TypeLoadMode.LoadFromPreBuiltAssembly)
            {
                await _container.GetInstance<DynamicCodeBuilder>().LoadPrebuiltTypes();
            }

            organizeTransports();

            assertNoUnknownTransportsInSubscribers(Options);
            assertNoUnknownTransportsInListeners(Options);

            foreach (var transport in Transports)
            {
                transport.InitializeSendersAndListeners(this);
            }

            GetOrBuild(TransportConstants.RetryUri);

            var durabilityLogger = _container.GetInstance<ILogger<DurabilityAgent>>();


            // HOKEY, BUT IT WORKS
            if (_container.Model.DefaultTypeFor<IEnvelopePersistence>() != typeof(NulloEnvelopePersistence))
            {
                // TODO -- use the worker queue for Retries?
                var worker = new DurableWorkerQueue(new ListenerSettings(), Pipeline, Settings, Persistence, TransportLogger);
                Durability = new DurabilityAgent(TransportLogger, durabilityLogger, worker, Persistence, this,
                    Options.Advanced);
                // TODO -- use the cancellation token from the app!
                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
        }

        public void AddSubscriber(ISubscriber subscriber)
        {
            _subscribers = _subscribers.AddOrUpdate(subscriber.Uri, subscriber);
        }

        public HandlerGraph Handlers { get; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // This is important!
            _container.As<Container>().DisposalLock = DisposalLock.Unlocked;

            return Durability.StopAsync(cancellationToken);


        }

        public string[] ValidTransports => _transports.Keys.ToArray();

        public ISubscriber GetOrBuild(Uri address)
        {
            assertValidTransport(address);

            if (_subscribers.TryFind(address, out var channel)) return channel;

            lock (_channelLock)
            {
                if (!_subscribers.TryFind(address, out channel))
                {
                    channel = buildChannel(address);
                    _subscribers = _subscribers.AddOrUpdate(address, channel);
                }

                return channel;
            }
        }

        public ISubscriber[] AllKnown()
        {
            return _subscribers.Enumerate().Select(x => x.Value).ToArray();
        }

        private void organizeTransports()
        {
            Transports
                .Each(t => _transports.Add(t.Protocol, t));

        }

        private void assertValidTransport(Uri uri)
        {
            if (!_transports.ContainsKey(uri.Scheme))
                throw new ArgumentOutOfRangeException(nameof(uri), $"Unrecognized transport scheme '{uri.Scheme}'");
        }

        private ISubscriber buildChannel(Uri uri)
        {
            assertValidTransport(uri);

            var transport = _transports[uri.Scheme];
            var agent = transport.BuildSendingAgent(uri, this, Settings.Cancellation);

            var subscriber = new Subscriber(uri, new Subscription[0]);
            subscriber.StartSending(Logger, agent, transport.ReplyUri);

            return subscriber;
        }

        private void assertNoUnknownTransportsInListeners(JasperOptions settings)
        {
            var unknowns = settings.Listeners.Where(x => !ValidTransports.Contains(x.Scheme)).ToArray();

            if (unknowns.Any())
                throw new UnknownTransportException(
                    $"Unknown transports referenced in listeners: {unknowns.Select(x => x.ToString()).Join(", ")}");
        }

        private void assertNoUnknownTransportsInSubscribers(JasperOptions settings)
        {
            var unknowns = settings.Subscriptions.Where(x => !ValidTransports.Contains(x.Uri.Scheme)).ToArray();
            if (unknowns.Length > 0)
                throw new UnknownTransportException(
                    $"Unknown transports referenced in {unknowns.Select(x => x.Uri.ToString()).Join(", ")}");
        }


        public void AddListener(ListenerSettings listenerSettings, IListener agent)
        {
            var worker = listenerSettings.IsDurable
                ? (IWorkerQueue) new DurableWorkerQueue(listenerSettings, Pipeline, Settings, Persistence,
                    TransportLogger)
                : new LightweightWorkerQueue(listenerSettings, TransportLogger, Pipeline, Settings);


            _listeners.Add(worker);

            worker.StartListening(agent);
        }
    }
}
