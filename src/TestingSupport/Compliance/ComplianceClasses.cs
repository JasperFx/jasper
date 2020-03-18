using System;
using Jasper;
using Microsoft.Extensions.Hosting;
using TestMessages;

namespace TestingSupport.Compliance
{
    public abstract class SendingCompliance : IDisposable
    {
        private IHost _sender;
        private IHost _receiver;

        public void SenderIs<T>() where T : JasperOptions, new()
        {
            _sender = JasperHost.For<T>();
        }

        public void ReceiverIs<T>() where T : JasperOptions, new()
        {
            _receiver = JasperHost.For<T>();
        }

        public void SenderIs(Action<JasperOptions> configure)
        {
            _sender = JasperHost.For(configure);
        }

        public void ReceiverIs(Action<JasperOptions> configure)
        {
            _receiver = JasperHost.For(configure);
        }

        public void Dispose()
        {
            _sender?.Dispose();
            _receiver?.Dispose();
        }
    }

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

    public class TimeoutsMessage
    {
    }
}
