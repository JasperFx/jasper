using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Transports.InMemory
{
    public class LoopbackSender : ISender
    {
        private readonly Uri _destination;
        private readonly ILoopbackQueue _queue;

        public LoopbackSender(Uri destination, ILoopbackQueue queue)
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
