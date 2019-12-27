using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Invocation;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Transports;
using Jasper.Transports.Sending;

namespace Jasper.Runtime
{
    // Replaces ISubscriberGraph
    public interface ITransportRuntime : IDisposable
    {
        ISendingAgent AddSubscriber(Uri replyUri, ISender sender, Endpoint endpoint);
        ISendingAgent GetOrBuildSendingAgent(Uri address);
        void AddListener(IListener listener, Endpoint settings);
        Task Stop();
        ISendingAgent[] FindSubscribers(Type messageType);
        void AddSubscriber(ISendingAgent replyUri, Subscription[] subscriptions);

        ISendingAgent AgentForLocalQueue(string queueName);
        ISendingAgent[] FindLocalSubscribers(Type messageType);
    }


    public interface IMessagingRoot : IDisposable
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IMessageRouter Router { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger MessageLogger { get; }
        MessagingSerializationGraph Serialization { get; }
        JasperOptions Options { get; }

        IMessageContext NewContext();
        IMessageContext ContextFor(Envelope envelope);

        IEnvelopePersistence Persistence { get; }
        ITransportLogger TransportLogger { get; }
        AdvancedSettings Settings { get; }
        ITransportRuntime Runtime { get; }
        CancellationToken Cancellation { get; }
    }
}
