using System;
using System.Collections.Generic;
using System.Threading;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime.Routing;
using Jasper.Runtime.Scheduled;
using Jasper.Serialization;
using Jasper.Serialization.New;

namespace Jasper.Runtime
{
    public interface IMessagingRoot : IDisposable
    {
        IScheduledJobProcessor ScheduledJobs { get; }
        IEnvelopeRouter Router { get; }
        IHandlerPipeline Pipeline { get; }
        IMessageLogger MessageLogger { get; }
        MessagingSerializationGraph Serialization { get; }
        JasperOptions Options { get; }

        IExecutionContext NewContext();
        IExecutionContext ContextFor(Envelope envelope);

        IEnvelopePersistence Persistence { get; }
        ITransportLogger TransportLogger { get; }
        AdvancedSettings Settings { get; }
        ITransportRuntime Runtime { get; }
        CancellationToken Cancellation { get; }

        IAcknowledgementSender Acknowledgements { get; }
    }
}
