using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Configuration;
using Jasper.Messaging.Logging;
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
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Jasper.Testing.Messaging
{
    public class MockMessagingRoot : IMessagingRoot
    {
        public IScheduledJobProcessor ScheduledJobs { get; } = Substitute.For<IScheduledJobProcessor>();
        public IMessageRouter Router { get; } = Substitute.For<IMessageRouter>();
        public UriAliasLookup Lookup { get; } = new UriAliasLookup(new IUriLookup[0]);
        public IWorkerQueue Workers { get; } = Substitute.For<IWorkerQueue>();
        public IHandlerPipeline Pipeline { get; } = Substitute.For<IHandlerPipeline>();
        public IMessageLogger Logger { get; } = new MessageLogger(new LoggerFactory());
        public MessagingSerializationGraph Serialization { get; } = MessagingSerializationGraph.Basic();
        public IReplyWatcher Replies { get; } = new ReplyWatcher();
        public MessagingSettings Settings { get; } = new MessagingSettings();
        public IDurableMessagingFactory Factory { get; } = Substitute.For<IDurableMessagingFactory>();

        public ITransport[] Transports { get; } = new ITransport[]{Substitute.For<ITransport>(), Substitute.For<ITransport>(), Substitute.For<ITransport>()};

        public IChannelGraph Channels { get; } = Substitute.For<IChannelGraph>();

        public IMessageContext NewContext()
        {
            return MessagingRoot.BusFor(this);
        }

        public Task Activate(LocalWorkerSender localWorker, CapabilityGraph capabilities, JasperRuntime runtime,
            GenerationRules generation, PerfTimer timer)
        {
            return Task.CompletedTask;
        }

        public IMessageContext ContextFor(Envelope envelope)
        {
            return MessagingRoot.BusFor(envelope, this);
        }
    }
}
