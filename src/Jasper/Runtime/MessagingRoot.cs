using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Handlers;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Runtime.WorkerQueues;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Local;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime
{
    public class MessagingRoot : IMessagingRoot, IHostedService
    {
        private readonly IContainer _container;

        private readonly Lazy<IEnvelopePersistence> _persistence;
        private bool _hasStopped;


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

            Cancellation = Settings.Cancellation;

        }

        public DurabilityAgent Durability { get; private set; }

        public void Dispose()
        {
            if (_hasStopped)
            {
                StopAsync(Settings.Cancellation).GetAwaiter().GetResult();
            }

            Settings.Cancel();

            Runtime.Dispose();

            ScheduledJobs.Dispose();


        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await bootstrap();
            }
            catch (Exception e)
            {
                MessageLogger.LogException(e, message: "Failed to start the Jasper messaging");
                throw;
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_hasStopped) return;

            _hasStopped = true;



            // This is important!
            _container.As<Container>().DisposalLock = DisposalLock.Unlocked;


            if (Durability != null) await Durability.StopAsync(cancellationToken);

            Settings.Cancel();
        }

        public ITransportRuntime Runtime { get; }
        public CancellationToken Cancellation { get; }

        public AdvancedSettings Settings { get; }

        public ITransportLogger TransportLogger { get; }

        public IScheduledJobProcessor ScheduledJobs { get; set; }

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


        public HandlerGraph Handlers { get; }

        private async Task bootstrap()
        {
            // Build up the message handlers
            await Handlers.Compiling;
            Handlers.Compile(Options.Advanced.CodeGeneration, _container);

            // If set, use pre-generated message handlers for quicker starts
            if (Options.Advanced.CodeGeneration.TypeLoadMode == TypeLoadMode.LoadFromPreBuiltAssembly)
            {
                await _container.GetInstance<DynamicCodeBuilder>().LoadPrebuiltTypes();
            }


            // Start all the listeners and senders
            Runtime.As<TransportRuntime>().Initialize();

            ScheduledJobs =
                new InMemoryScheduledJobProcessor((IWorkerQueue) Runtime.AgentForLocalQueue(TransportConstants.Replies));

            switch (Settings.StorageProvisioning)
            {
                case StorageProvisioning.Rebuild:
                    Persistence.Admin.RebuildSchemaObjects();
                    break;

                case StorageProvisioning.Clear:
                    Persistence.Admin.ClearAllPersistedEnvelopes();
                    break;
            }

            await startDurabilityAgent();
        }

        private async Task startDurabilityAgent()
        {
            // HOKEY, BUT IT WORKS
            if (_container.Model.DefaultTypeFor<IEnvelopePersistence>() != typeof(NulloEnvelopePersistence))
            {
                var durabilityLogger = _container.GetInstance<ILogger<DurabilityAgent>>();

                // TODO -- use the worker queue for Retries?
                var worker = new DurableWorkerQueue(new LocalQueueSettings("scheduled"), Pipeline, Settings, Persistence,
                    TransportLogger);
                Durability = new DurabilityAgent(TransportLogger, durabilityLogger, worker, Persistence, Runtime,
                    Options.Advanced);

                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
        }
    }
}
