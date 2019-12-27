using System;
using TestMessages;

namespace Jasper.Testing.Transports.Tcp
{
    public class MessageConsumer
    {


        public void Consume(Envelope envelope, Message1 message)
        {
        }

        public void Consume(Envelope envelope, Message2 message)
        {
            if (envelope.Attempts < 2) throw new DivideByZeroException();

        }

        public void Consume(Envelope envelope, TimeoutsMessage message)
        {
            if (envelope.Attempts < 2) throw new TimeoutException();

        }
    }
}
