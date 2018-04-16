using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.RabbitMQ
{
    public class RabbitMQSender : ISender
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start(ISenderCallback callback)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Uri Destination { get; }
        public int QueuedCount { get; }
        public bool Latched { get; }
        public Task LatchAndDrain()
        {
            throw new NotImplementedException();
        }

        public void Unlatch()
        {
            throw new NotImplementedException();
        }

        public Task Ping()
        {
            throw new NotImplementedException();
        }
    }
}