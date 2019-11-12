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
        private readonly ITransportLogger _transportLogger;
        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;


        private readonly Lazy<IEnvelopePersistence> _persistence;
        private readonly IContainer _container;

        public MessagingRoot(MessagingSerializationGraph serialization,
            JasperOptions options,
            HandlerGraph handlers,
            ISubscriberGraph subscribers,
            IMessageLogger messageLogger,
            IContainer container,
            ITransportLogger transportLogger
            )
        {
            Options = options;
            Handlers = handlers;
            _transportLogger = transportLogger;
            Subscribers = subscribers;
            Transports = container.QuickBuildAll<ITransport>().ToArray();


            Serialization = serialization;

            Logger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, handlers, Logger,
                container.QuickBuildAll<IMissingHandler>(),
                this);

            Workers = new WorkerQueue(Logger, Pipeline, options.Advanced);

            Router = new MessageRouter(this, handlers);

            _persistence = new Lazy<IEnvelopePersistence>(() => container.GetInstance<IEnvelopePersistence>());

            _container = container;
        }

        public void Dispose()
        {
            ScheduledJobs.Dispose();
        }

        public DurabilityAgent Durability { get; private set; }

        public ListeningStatus ListeningStatus
        {
            get => _listeningStatus;
            set
            {
                _transportLogger.ListeningStatusChange(value);
                _listeningStatus = value;


                foreach (var transport in Transports) transport.ListeningStatus = value;
            }
        }

        public ITransport[] Transports { get; }

        public IScheduledJobProcessor ScheduledJobs => Workers.ScheduledJobs;

        public JasperOptions Options { get; }

        public ISubscriberGraph Subscribers { get; }

        public IMessageRouter Router { get; }

        [Obsolete]
        public IWorkerQueue Workers { get; }

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
                await Handlers.Compiling;

                Handlers.Compile(Options.CodeGeneration, _container);


                Handlers.Workers.Compile(Handlers.Chains.Select(x => x.MessageType));



                if (Options.CodeGeneration.TypeLoadMode == TypeLoadMode.LoadFromPreBuiltAssembly)
                {
                    await _container.GetInstance<DynamicCodeBuilder>().LoadPrebuiltTypes();
                }

                ((SubscriberGraph) Subscribers).Start(this);

                var durabilityLogger = _container.GetInstance<ILogger<DurabilityAgent>>();
                Durability = new DurabilityAgent(_transportLogger, durabilityLogger, Workers, Persistence, Subscribers, Options.Advanced);
                // TODO -- use the cancellation token from the app!
                await Durability.StartAsync(Options.Advanced.Cancellation);
            }
            catch (Exception e)
            {
                Logger.LogException(e, message:"Failed to start the Jasper messaging");
                throw;
            }
        }

        public HandlerGraph Handlers { get; }


        [Obsolete("Get rid of this")]
        public bool ShouldBeDurable(Type messageType)
        {
            return Handlers.Workers.ShouldBeDurable(messageType);
        }

        public ISendingAgent BuildDurableSendingAgent(Uri destination, ISender sender)
        {
            return new DurableSendingAgent(destination, sender, _transportLogger, Options.Advanced, Persistence);
        }

        public ISendingAgent BuildDurableLoopbackAgent(Uri destination)
        {
            return new DurableLoopbackSendingAgent(destination, Workers, Persistence, Serialization, _transportLogger, Options.Advanced);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            // This is important!
            _container.As<Container>().DisposalLock = DisposalLock.Unlocked;

            return Durability.StopAsync(cancellationToken);


        }
    }
}
