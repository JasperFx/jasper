using System;

namespace Jasper.Bus.Transports.InMemory
{
    public class InMemorySettings
    {
        public Uri DefaultReplyUri { get; set; } = new Uri("memory://replies");
        public int BufferCapacity { get; set; } = 100;
    }
}
