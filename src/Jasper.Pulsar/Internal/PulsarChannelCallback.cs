using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Transports;

namespace Jasper.Pulsar.Internal
{
    class PulsarChannelCallback : IChannelCallback
    {
        private readonly Message _message;
        private readonly IConsumer _consumer;
        public PulsarChannelCallback(Message message, IConsumer consumer)
        {
            _message = message;
            _consumer = consumer;
        }

        public Task Complete(Envelope envelope) => _consumer.Acknowledge(_message).AsTask();

        public Task Defer(Envelope envelope) => Task.CompletedTask;
    }
}
