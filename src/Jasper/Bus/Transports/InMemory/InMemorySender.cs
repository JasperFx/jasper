using System;
using System.Threading.Tasks;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.InMemory
{
    public class InMemorySender : ISender
    {
        private readonly Uri _destination;
        private readonly IInMemoryQueue _queue;

        public InMemorySender(Uri destination, IInMemoryQueue queue)
        {
            _queue = queue;
            _destination = destination;
        }

        public Task Send(Envelope envelope)
        {
            return _queue.Send(envelope, _destination);
        }
    }
}
