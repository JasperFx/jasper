using System;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using TestMessages;

namespace Jasper.Testing.Messaging.Transports.Tcp
{
    public class MessageConsumer
    {
        private readonly MessageTracker _tracker;

        public MessageConsumer(MessageTracker tracker)
        {
            _tracker = tracker;
        }

        public void Consume(Envelope envelope, Message1 message)
        {
            _tracker.Record(message, envelope);
        }

        public void Consume(Envelope envelope, Message2 message)
        {
            if (envelope.Attempts < 2) throw new DivideByZeroException();

            _tracker.Record(message, envelope);
        }

        public void Consume(Envelope envelope, TimeoutsMessage message)
        {
            if (envelope.Attempts < 2) throw new TimeoutException();

            _tracker.Record(message, envelope);
        }
    }
}
