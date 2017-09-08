using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Core;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class determining_queue_name
    {
        [Fact]
        public void use_the_last_segment_if_it_exists()
        {
            "tcp://localhost:2222/incoming".ToUri().QueueName().ShouldBe("incoming");
        }

        [Fact]
        public void fall_back_to_the_default_queue_if_no_segments()
        {
            "tcp://localhost:2222".ToUri().QueueName().ShouldBe(TransportConstants.Default);
        }
    }
}
