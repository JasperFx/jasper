using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Delayed;

namespace Jasper.Bus.Runtime.Invocation
{
    public interface IEnvelopeContext : IInvocationContext, IDisposable
    {
        void SendAllQueuedOutgoingMessages();

        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);

        void SendOutgoingMessage(Envelope original, object cascadingMessage);

        void SendFailureAcknowledgement(Envelope original, string message);

        // doesn't need to be passed the envelope here, but maybe leave this one
        Task Retry(Envelope envelope);

        IBusLogger Logger { get; }
        IDelayedJobProcessor DelayedJobs { get; }
    }
}
