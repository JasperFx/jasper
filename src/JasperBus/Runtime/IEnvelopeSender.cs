using System.Collections.Generic;

namespace JasperBus.Runtime
{
    // Also includes what was IOutgoingSender in fubu
    public interface IEnvelopeSender
    {
        string Send(Envelope envelope);
        string Send(Envelope envelope, IMessageCallback callback);

        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);
        void SendFailureAcknowledgement(Envelope original, string message);

    }
}