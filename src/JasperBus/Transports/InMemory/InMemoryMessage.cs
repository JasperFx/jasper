using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JasperBus.Model;

namespace JasperBus.Transports.InMemory
{
    public class InMemoryMessage
    {
        public byte[] Data { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Guid Id { get; set; }
        public string Queue { get; set; }
        public DateTime SentAt { get; set; }
    }
}
