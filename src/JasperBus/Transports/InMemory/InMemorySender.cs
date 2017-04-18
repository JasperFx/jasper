using System;
using System.Collections.Generic;
using JasperBus.Configuration;
using System.Threading.Tasks;

namespace JasperBus.Transports.InMemory
{
    public class InMemorySender : ISender
    {
        private readonly Uri _destination;
        private readonly InMemoryQueue _queue;
        private readonly string _subQueue;

        public InMemorySender(Uri destination, InMemoryQueue queue, string subQueue)
        {
            _destination = destination;
            _queue = queue;
            _subQueue = subQueue;
        }

        public Task Send(byte[] data, IDictionary<string, string> headers)
        {
            return _queue.Send(data, headers, _destination, _subQueue);
        }
    }
}
