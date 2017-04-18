using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JasperBus.Transports.InMemory
{
    public class InMemorySettings
    {
        public Uri DefaultReplyUri { get; set; } = new Uri("memory://localhost:2345/replies");
    }
}
