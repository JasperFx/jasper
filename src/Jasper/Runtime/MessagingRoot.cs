using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.ErrorHandling;
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

            // TODO -- might make NoHandlerContinuation lazy!
            Pipeline = new HandlerPipeline(Serialization, Handlers, MessageLogger,
                new NoHandlerContinuation(container.GetAllInstances<IMissingHandler>().ToArray(), this),
                this);

            Runtime = new TransportRuntime(this);


            _persistence = new Lazy<IEnvelopePersistence>(container.GetInstance<IEnvelopePersistence>);

            Router = new EnvelopeRouter(this);

            Acknowledgements = new AcknowledgementSender(Router, Serialization);

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

        public IAcknowledgementSender Acknowledgements { get; }

        public ITransportRuntime Runtime { get; }
        public CancellationToken Cancellation { get; }

        public AdvancedSettings Settings { get; }

        public ITransportLogger TransportLogger { get; }

        public IScheduledJobProcessor ScheduledJobs { get; set; }

        public JasperOptions Options { get; }

        public IEnvelopeRouter Router { get; }

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
            // TODO -- make this take in the callback as well
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

            // Bit of a hack, but it's necessary. Came up in compliance tests
            if (Persistence is NulloEnvelopePersistence p) p.ScheduledJobs = ScheduledJobs;

            switch (Settings.StorageProvisioning)
            {
                case StorageProvisioning.Rebuild:
                    await Persistence.Admin.RebuildSchemaObjects();
                    break;

                case StorageProvisioning.Clear:
                    await Persistence.Admin.ClearAllPersistedEnvelopes();
                    break;
            }

            await startDurabilityAgent();
        }

        private async Task startDurabilityAgent()
        {
            // HOKEY, BUT IT WORKS
            if (_container.Model.DefaultTypeFor<IEnvelopePersistence>() != typeof(NulloEnvelopePersistence) && Options.Advanced.DurabilityAgentEnabled)
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
