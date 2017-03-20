using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JasperBus.Runtime.Invocation
{
    public interface IEnvelopeContext : IInvocationContext, IDisposable
    {
        void SendAllQueuedOutgoingMessages();

        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);

        void SendOutgoingMessage(Envelope original, object cascadingMessage);

        void SendFailureAcknowledgement(Envelope original, string message);

        void Error(string correlationId, string message, Exception exception);

        // doesn't need to be passed the envelope here, but maybe leave this one
        Task Retry(Envelope envelope);

        IBusLogger Logger { get; }
    }
}