using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Runtime.Routing;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using LamarCompiler.Util;

namespace Jasper.Messaging
{
    public interface IMessagingRoot
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IMessageRouter Router { get; }
        IWorkerQueue Workers { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger Logger { get; }
        MessagingSerializationGraph Serialization { get; }
        ISubscriberGraph Subscribers { get; }
        JasperOptions Settings { get; }
        IDurableMessagingFactory Factory { get; }
        ITransport[] Transports { get; }
        ListeningStatus ListeningStatus { get; set; }

        IMessageContext NewContext();
        IMessageContext ContextFor(Envelope envelope);

        void Activate(LocalWorkerSender localWorker,
            JasperGenerationRules generation, IContainer container);

        void ApplyMessageTypeSpecificRules(Envelope envelope);
        bool ShouldBeDurable(Type messageType);
    }
}
