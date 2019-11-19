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
    public class MessagingRoot : IDisposable, IMessagingRoot, IHostedService
    {
        [Obsolete("")]
        private readonly IList<IListeningWorkerQueue> _listeners = new List<IListeningWorkerQueue>();

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

            Settings = options.Advanced;
            Serialization = serialization;

            MessageLogger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, Handlers, MessageLogger,
                container.QuickBuildAll<IMissingHandler>(),
                this);

            Runtime = new TransportRuntime(this);
            Router = new MessageRouter(Handlers, serialization, Options.Advanced, Runtime);

            _persistence = new Lazy<IEnvelopePersistence>(() => container.GetInstance<IEnvelopePersistence>());

            _container = container;


            ScheduledJobs = new InMemoryScheduledJobProcessor(new LightweightWorkerQueue(new ListenerSettings(), transportLogger, Pipeline, Settings));

        }

        public void Dispose()
        {
            foreach (var listener in _listeners) listener.SafeDispose();

            _listeners.Clear();

            Runtime.Dispose();

            ScheduledJobs.Dispose();
        }

        public ITransportRuntime Runtime { get; }

        public AdvancedSettings Settings { get; }

        public ITransportLogger TransportLogger { get; }

        public DurabilityAgent Durability { get; private set; }

        public IScheduledJobProcessor ScheduledJobs { get; }

        public JasperOptions Options { get; }

        public IMessageRouter Router { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger MessageLogger { get; }

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
                MessageLogger.LogException(e, message:"Failed to start the Jasper messaging");
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


            // Start all the listeners and senders
            Runtime.As<TransportRuntime>().Initialize();

            await startDurabilityAgent();
        }

        private async Task startDurabilityAgent()
        {
            // HOKEY, BUT IT WORKS
            if (_container.Model.DefaultTypeFor<IEnvelopePersistence>() != typeof(NulloEnvelopePersistence))
            {
                var durabilityLogger = _container.GetInstance<ILogger<DurabilityAgent>>();

                // TODO -- use the worker queue for Retries?
                var worker = new DurableWorkerQueue(new ListenerSettings(), Pipeline, Settings, Persistence, TransportLogger);
                Durability = new DurabilityAgent(TransportLogger, durabilityLogger, worker, Persistence, Runtime,
                    Options.Advanced);
                // TODO -- use the cancellation token from the app!
                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
        }


        public HandlerGraph Handlers { get; }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // This is important!
            _container.As<Container>().DisposalLock = DisposalLock.Unlocked;

            return Durability.StopAsync(cancellationToken);


        }

    }
}
