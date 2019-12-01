using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
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
using Jasper.Messaging.Transports.Local;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging
{
    public class MessagingRoot : IDisposable, IMessagingRoot, IHostedService
    {
        private readonly IContainer _container;

        private readonly Lazy<IEnvelopePersistence> _persistence;

        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;

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



        }

        public DurabilityAgent Durability { get; private set; }

        public void Dispose()
        {
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

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // This is important!
            _container.As<Container>().DisposalLock = DisposalLock.Unlocked;

            return Durability.StopAsync(cancellationToken);
        }

        public ITransportRuntime Runtime { get; }

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
            Handlers.Compile(Options.CodeGeneration, _container);

            // If set, use pre-generated message handlers for quicker starts
            if (Options.CodeGeneration.TypeLoadMode == TypeLoadMode.LoadFromPreBuiltAssembly)
            {
                await _container.GetInstance<DynamicCodeBuilder>().LoadPrebuiltTypes();
            }


            // Start all the listeners and senders
            Runtime.As<TransportRuntime>().Initialize();

            ScheduledJobs =
                new InMemoryScheduledJobProcessor((IWorkerQueue) Runtime.AgentForLocalQueue(TransportConstants.Replies));

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
                // TODO -- use the cancellation token from the app!
                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
        }
    }
}
