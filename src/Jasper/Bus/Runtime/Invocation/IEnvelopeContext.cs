using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Logging;

namespace Jasper.Bus.Runtime.Invocation
{
    public interface IEnvelopeContext : IInvocationContext, IDisposable
    {
        Task SendAllQueuedOutgoingMessages();

        Task SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);

        Task SendOutgoingMessage(Envelope original, object cascadingMessage);

        Task SendFailureAcknowledgement(Envelope original, string message);

        // doesn't need to be passed the envelope here, but maybe leave this one
        Task Retry(Envelope envelope);

        IBusLogger Logger { get; }

        Task SendAcknowledgement(Envelope original);
    }
}
