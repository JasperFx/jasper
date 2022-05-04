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
    public interface IJasperRuntime
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

        IJasperEndpoints Endpoints { get; }

    }

    public interface IJasperEndpoints
    {
        ISendingAgent AddSubscriber(Uri? replyUri, ISender sender, Endpoint endpoint);

        ISendingAgent GetOrBuildSendingAgent(Uri address);
        void AddListener(IListener listener, Endpoint settings);

        void AddSendingAgent(ISendingAgent sendingAgent);

        void AddSubscriber(ISubscriber subscriber);

        Endpoint? For(Uri uri);
    }
}
