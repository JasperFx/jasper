using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JasperBus.Transports.InMemory
{
    public class InMemorySettings
    {
        public Uri DefaultReplyUri { get; set; } = new Uri("memory://replies");
        public int BufferCapacity { get; set; } = 100;
    }
}
