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
    // Replaces ISubscriberGraph
    public interface ITransportRuntime : IDisposable
    {
        ISendingAgent AddSubscriber(Uri replyUri, ISender sender, Subscription[] subscriptions);
        ISendingAgent GetOrBuildSendingAgent(Uri address);
        void AddListener(IListener listener, Endpoint settings);
        Task Stop();
        ISendingAgent[] FindSubscribers(Type messageType);
        void AddSubscriber(ISendingAgent replyUri, Subscription[] subscriptions);

        ISendingAgent AgentForLocalQueue(string queueName);
    }


    public interface IMessagingRoot
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IMessageRouter Router { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger MessageLogger { get; }
        MessagingSerializationGraph Serialization { get; }
        JasperOptions Options { get; }


        HandlerGraph Handlers { get; }

        IMessageContext NewContext();
        IMessageContext ContextFor(Envelope envelope);

        IEnvelopePersistence Persistence { get; }
        ITransportLogger TransportLogger { get; }
        AdvancedSettings Settings { get; }
        ITransportRuntime Runtime { get; }
    }
}
