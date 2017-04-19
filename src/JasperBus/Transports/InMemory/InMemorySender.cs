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
        
        public InMemorySender(Uri destination, InMemoryQueue queue)
        {
            _queue = queue;
            _destination = destination;
        }

        public Task Send(byte[] data, IDictionary<string, string> headers)
        {
            return _queue.Send(data, headers, _destination);
        }
    }
}
