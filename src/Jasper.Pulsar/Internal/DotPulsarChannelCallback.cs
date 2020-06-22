using System;
using System.Threading.Tasks;
using DotPulsar;
using DotPulsar.Abstractions;
using Jasper.Transports;

namespace Jasper.DotPulsar.Internal
{
    class DotPulsarChannelCallback : IChannelCallback
    {
        private readonly IConsumer _consumer;
        private readonly MessageId _messageId;
        public DotPulsarChannelCallback(MessageId messageId, IConsumer consumer)
        {
            _messageId = messageId;
            _consumer = consumer;
        }

        public Task Complete(Envelope envelope) =>  _consumer.Acknowledge(_messageId).AsTask();

        public Task Defer(Envelope envelope)
        {
            return Task.CompletedTask;
        }
    }
}
