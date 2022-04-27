using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Jasper.Transports.Sending;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime
{
    public interface IJasperRuntime : IDisposable
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IEnvelopeRouter Router { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger MessageLogger { get; }
        JasperOptions Options { get; }

        IEnvelopePersistence Persistence { get; }
        ILogger Logger { get; }
        AdvancedSettings Advanced { get; }
        CancellationToken Cancellation { get; }

        IAcknowledgementSender Acknowledgements { get; }


        // TODO -- can this be hidden from the public interface?
        Type DetermineMessageType(Envelope envelope);

        // TODO -- can this be hidden from the public interface?
        void RegisterMessageType(Type messageType);

        ISendingAgent AddSubscriber(Uri? replyUri, ISender sender, Endpoint endpoint);

        ISendingAgent GetOrBuildSendingAgent(Uri address);
        void AddListener(IListener listener, Endpoint settings);

        IEnumerable<ISubscriber> Subscribers { get; }

        void AddSendingAgent(ISendingAgent sendingAgent);

        void AddSubscriber(ISubscriber subscriber);

        ISendingAgent AgentForLocalQueue(string queueName);
        Endpoint? EndpointFor(Uri uri);
    }
}
