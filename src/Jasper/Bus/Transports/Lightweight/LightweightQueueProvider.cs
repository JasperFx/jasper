using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Loopback;

namespace Jasper.Bus.Transports.Lightweight
{
    public class LightweightQueueProvider : IQueueProvider
    {
        private readonly Lazy<IChannel> _retryChannel;

        public LightweightQueueProvider(Func<IChannel> retryChannel)
        {
            _retryChannel = new Lazy<IChannel>(retryChannel);
        }

        public IMessageCallback BuildCallback(Envelope envelope, QueueReceiver receiver)
        {
            return new LightweightCallback(_retryChannel.Value);
        }

        public void StoreIncomingMessages(Envelope[] messages)
        {
            // nothing
        }

        public void RemoveIncomingMessages(Envelope[] messages)
        {
            // nothing
        }
    }
}
