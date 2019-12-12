using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;

namespace Jasper.Messaging.Transports.Sending
{
    public class NulloSender : ISender
    {
        private ISenderCallback _callback;

        public NulloSender(Uri destination)
        {
            Destination = destination;
        }

        public void Dispose()
        {
        }

        public Uri Destination { get; }
        public int QueuedCount => 0;
        public bool Latched => false;
        public void Start(ISenderCallback callback)
        {
            _callback = callback;
        }

        public Task Enqueue(Envelope envelope)
        {
            _callback.Successful(envelope);
            return Task.CompletedTask;
        }

        public Task LatchAndDrain()
        {
            return Task.CompletedTask;
        }

        public void Unlatch()
        {

        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }

        public bool SupportsNativeScheduledSend { get; } = false;
    }
}
