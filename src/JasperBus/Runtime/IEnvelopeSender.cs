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

    public class EnvelopeSender : IEnvelopeSender
    {
        public string Send(Envelope envelope)
        {
            throw new System.NotImplementedException();
        }

        public string Send(Envelope envelope, IMessageCallback callback)
        {
            throw new System.NotImplementedException();
        }

        public void SendOutgoingMessages(Envelope original, IEnumerable<object> cascadingMessages)
        {
            throw new System.NotImplementedException();
        }

        public void SendFailureAcknowledgement(Envelope original, string message)
        {
            throw new System.NotImplementedException();
        }
    }
}