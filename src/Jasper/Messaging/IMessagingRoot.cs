using System.Threading.Tasks;
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

namespace Jasper.Messaging
{
    public interface IMessagingRoot
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IMessageRouter Router { get; }
        UriAliasLookup Lookup { get; }
        IWorkerQueue Workers { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger Logger { get; }
        MessagingSerializationGraph Serialization { get; }
        IReplyWatcher Replies { get; }
        IChannelGraph Channels { get; }
        MessagingSettings Settings { get; }
        IDurableMessagingFactory Factory { get; }
        ITransport[] Transports { get; }

        IMessageContext NewContext();
        IMessageContext ContextFor(Envelope envelope);

        Task Activate(LocalWorkerSender localWorker, CapabilityGraph capabilities, JasperRuntime runtime,
            GenerationRules generation, PerfTimer timer);
    }
}