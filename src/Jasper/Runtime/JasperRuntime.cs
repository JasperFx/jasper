using System;
using System.Collections.Generic;
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
using Jasper.Util;
using Lamar;
using LamarCodeGeneration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Oakton.Resources;

namespace Jasper.Runtime
{
    public class JasperRuntime : PooledObjectPolicy<ExecutionContext>, IJasperRuntime, IHostedService, IStatefulResourceSource
    {
        private readonly IContainer _container;

        private readonly Lazy<IEnvelopePersistence?> _persistence;
        private bool _hasStopped;


        public JasperRuntime(JasperOptions options,
            IMessageLogger messageLogger,
            IContainer container,
            ILogger<JasperRuntime> logger)
        {
            Settings = options.Advanced;
            Options = options;
            Options.Serializers.Add(new NewtonsoftSerializer(Settings.JsonSerialization));
            Handlers = options.HandlerGraph;
            Logger = logger;

            MessageLogger = messageLogger;

            var provider = container.GetInstance<ObjectPoolProvider>();
            var pool = provider.Create(this);

            // TODO -- might make NoHandlerContinuation lazy!
            Pipeline = new HandlerPipeline(Handlers, MessageLogger,
                new NoHandlerContinuation(container.GetAllInstances<IMissingHandler>().ToArray(), this),
                this, pool);

            Runtime = new TransportRuntime(this);

            _persistence = new Lazy<IEnvelopePersistence?>(container.GetInstance<IEnvelopePersistence>);

            Router = new EnvelopeRouter(this);

            Acknowledgements = new AcknowledgementSender(Router, this);

            _container = container;

            Cancellation = Settings.Cancellation;


        }

        public override ExecutionContext Create()
        {
            return new ExecutionContext(this);
        }

        public override bool Return(ExecutionContext context)
        {
            context.ClearState();
            return true;
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

            if (ScheduledJobs != null)
            {
                ScheduledJobs.Dispose();
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            try
            {
                await bootstrap();
            }
            catch (Exception? e)
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
        public bool TryFindMessageType(string? messageTypeName, out Type messageType)
        {
            return Handlers.TryFindMessageType(messageTypeName, out messageType);
        }

        public Type DetermineMessageType(Envelope? envelope)
        {
            if (envelope.Message == null)
            {
                if (TryFindMessageType(envelope.MessageType, out var messageType))
                {
                    return messageType;
                }

                throw new InvalidOperationException($"Unable to determine a message type for `{envelope.MessageType}`, the known types are: {Handlers.Chains.Select(x => x.MessageType.ToMessageTypeName()).Join(", ")}");
            }

            if (envelope.Message == null) throw new ArgumentNullException(nameof(Envelope.Message));
            return envelope.Message.GetType();
        }

        public void RegisterMessageType(Type messageType)
        {
            Handlers.RegisterMessageType(messageType);
        }

        public ITransportRuntime Runtime { get; }
        public CancellationToken Cancellation { get; }

        public AdvancedSettings? Settings { get; }

        public ILogger Logger { get; }

        public IScheduledJobProcessor ScheduledJobs { get; set; }

        public JasperOptions Options { get; }

        public IEnvelopeRouter Router { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger MessageLogger { get; }


        public IEnvelopePersistence? Persistence => _persistence.Value;

        public IExecutionContext NewContext()
        {
            return new ExecutionContext(this);
        }

        public IExecutionContext ContextFor(Envelope? envelope)
        {
            var context =  new ExecutionContext(this);
            context.ReadEnvelope(envelope, InvocationCallback.Instance);

            return context;
        }


        public HandlerGraph Handlers { get; }

        private async Task bootstrap()
        {
            // Build up the message handlers
            await Handlers.CompileAsync(Options, _container);

            // If set, use pre-generated message handlers for quicker starts
            if (Options.Advanced.CodeGeneration.TypeLoadMode == TypeLoadMode.Static)
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
                    await Persistence.Admin.RebuildStorageAsync();
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
                    Logger);
                Durability = new DurabilityAgent(Logger, durabilityLogger, worker, Persistence, Runtime,
                    Options.Advanced);

                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
        }


        IReadOnlyList<IStatefulResource> IStatefulResourceSource.FindResources()
        {
            var list = new List<IStatefulResource>();
            list.AddRange(Options.OfType<IStatefulResource>());

            return list;
        }
    }
}
