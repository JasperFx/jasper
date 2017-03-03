using System.Collections.Generic;

namespace JasperBus.Runtime
{
    public interface IEnvelopeSender
    {
        string Send(Envelope envelope);
        string Send(Envelope envelope, IMessageCallback callback);
    }

    public interface IOutgoingSender
    {
        void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages);
        void SendFailureAcknowledgement(Envelope original, string message);

        void Send(Envelope envelope);
    }


}