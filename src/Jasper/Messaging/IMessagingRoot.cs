using System;
using System.Threading;
using System.Threading.Tasks;
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
using Lamar;
using LamarCodeGeneration;

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
        JasperOptions Options { get; }


        ITransport[] Transports { get; }
        ListeningStatus ListeningStatus { get; set; }
        HandlerGraph Handlers { get; }

        IMessageContext NewContext();
        IMessageContext ContextFor(Envelope envelope);

        IEnvelopePersistence Persistence { get; }

        Task StartAsync(CancellationToken cancellationToken);

        void ApplyMessageTypeSpecificRules(Envelope envelope);
        bool ShouldBeDurable(Type messageType);

        ISendingAgent BuildDurableSendingAgent(Uri destination, ISender sender);
        ISendingAgent BuildDurableLoopbackAgent(Uri destination);


        Task StopAsync(CancellationToken cancellationToken);
    }
}
