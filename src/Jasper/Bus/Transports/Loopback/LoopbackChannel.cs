﻿using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;

namespace Jasper.Bus.Transports.Loopback
{
    public class LoopbackChannel : ChannelBase
    {
        private readonly QueueReceiver _receiver;

        public LoopbackChannel(SubscriberAddress address, QueueReceiver receiver) : base(address, TransportConstants.RepliesUri)
        {
            _receiver = receiver;
        }

        protected override Task send(Envelope envelope)
        {
            envelope.ReceivedAt = Destination;
            _receiver.Enqueue(envelope);
            return Task.CompletedTask;
        }
    }
}
