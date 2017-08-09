using System;

namespace Jasper.Bus.Transports.InMemory
{
    public class LoopbackSettings
    {
        public Uri DefaultReplyUri { get; set; } = new Uri("loopback://replies");
        public int BufferCapacity { get; set; } = 100;
    }
}
