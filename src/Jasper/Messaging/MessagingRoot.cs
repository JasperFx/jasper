using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Lamar.Codegen;
using Lamar.Util;

namespace Jasper.Messaging
{
    [CacheResolver]
    public class MessagingRoot : IDisposable, IMessagingRoot
    {
        // bouncing through this makes the mock root easier
        public static IMessageContext BusFor(Envelope envelope, IMessagingRoot root)
        {
            return new MessageContext(root.Router, root.Replies, root.Pipeline, root.Serialization, root.Settings, root.Channels, root.Factory, root.Logger, envelope);

        }

        // bouncing through this makes the mock root easier
        public static IMessageContext BusFor(IMessagingRoot root)
        {
            return new MessageContext(root.Router, root.Replies, root.Pipeline, root.Serialization, root.Settings, root.Channels, root.Factory, root.Logger);
        }

        private readonly HandlerGraph _handlers;
        private readonly ITransportLogger _transportLogger;
        private ListeningStatus _listeningStatus = ListeningStatus.Accepting;

        public MessagingRoot(
            MessagingSerializationGraph serialization,
            MessagingSettings settings,
            HandlerGraph handlers,
            IDurableMessagingFactory factory,
            IChannelGraph channels,
            ISubscriptionsRepository subscriptions,
            IMessageLogger messageLogger,
            Lamar.IContainer container,
            ITransportLogger transportLogger)
        {
            Settings = settings;
            _handlers = handlers;
            _transportLogger = transportLogger;
            Replies = new ReplyWatcher();
            Factory = factory;
            Channels = channels;
            Transports = container.QuickBuildAll<ITransport>().ToArray();



            Lookup = new UriAliasLookup(container.QuickBuildAll<IUriLookup>());


            Serialization = serialization;

            Logger = messageLogger;

            Pipeline = new HandlerPipeline(Serialization, handlers, Replies, Logger, container.QuickBuildAll<IMissingHandler>(),
                this);

            Workers = new WorkerQueue(Logger, Pipeline, settings);

            Router = new MessageRouter(Serialization, channels, subscriptions, handlers, Logger, Lookup, settings);

            // TODO -- ZOMG this is horrible, and I admit it.
            if (Factory is NulloDurableMessagingFactory f)
            {
                f.ScheduledJobs = ScheduledJobs;
            }
        }

        public ListeningStatus ListeningStatus
        {
            get => _listeningStatus;
            set {

                _transportLogger.ListeningStatusChange(value);
                _listeningStatus = value;


                foreach (var transport in Transports)
                {
                    transport.ListeningStatus = value;
                }
            }
        }

        public ITransport[] Transports { get; }

        public IScheduledJobProcessor ScheduledJobs => Workers.ScheduledJobs;

        public MessagingSettings Settings { get; }

        public IChannelGraph Channels { get; }

        public IReplyWatcher Replies { get; }

        public IMessageRouter Router { get; }

        public UriAliasLookup Lookup { get; }

        public IWorkerQueue Workers { get; }

        public IHandlerPipeline Pipeline { get; }

        public IMessageLogger Logger { get; }

        public MessagingSerializationGraph Serialization { get; }

        public IDurableMessagingFactory Factory { get; }

        public IMessageContext NewContext()
        {
            return BusFor(this);
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return BusFor(envelope, this);
        }

        public async Task Activate(LocalWorkerSender localWorker, CapabilityGraph capabilities, JasperRuntime runtime,
            GenerationRules generation, PerfTimer timer)
        {
            timer.MarkStart("ServiceBusActivator");

            _handlers.Compile(generation, runtime, timer);


            var capabilityCompilation = capabilities.Compile(_handlers, Serialization, Channels, runtime, Transports, Lookup);



            timer.Record("WorkersGraph.Compile", () =>
            {
                Settings.Workers.Compile(_handlers.Chains.Select(x => x.MessageType));
            });



            localWorker.Start(this);

            if (!Settings.DisableAllTransports)
            {
                timer.MarkStart("ApplyLookups");

                await Settings.ApplyLookups(Lookup);

                timer.MarkFinished("ApplyLookups");


                timer.Record("ChannelGraph.Start",
                    () => { ((ChannelGraph)Channels).Start(this, capabilities); });

            }

            runtime.Capabilities = await capabilityCompilation;
            if (runtime.Capabilities.Errors.Any() && Settings.ThrowOnValidationErrors)
            {
                throw new InvalidSubscriptionException(runtime.Capabilities.Errors);
            }

            timer.MarkFinished("ServiceBusActivator");
        }

        public void Dispose()
        {
            ScheduledJobs.Dispose();


        }
    }
}
