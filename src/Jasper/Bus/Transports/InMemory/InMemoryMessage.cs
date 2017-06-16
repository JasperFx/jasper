using System;
using System.Collections.Generic;

namespace Jasper.Bus.Transports.InMemory
{
    public class InMemoryMessage
    {
        public byte[] Data { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Guid Id { get; set; }
        public DateTime SentAt { get; set; }
    }
}
