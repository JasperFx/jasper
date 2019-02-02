using Jasper.Messaging.Transports;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace MessagingTests
{
    public class determining_queue_name_and_is_durable
    {
        [Fact]
        public void converting_from_old_durable_scheme()
        {
            "durable://localhost:2200".ToUri().ToCanonicalTcpUri()
                .ShouldBe("tcp://localhost:2200/durable".ToUri());

            "durable://localhost:2200/one".ToUri().ToCanonicalTcpUri()
                .ShouldBe("tcp://localhost:2200/durable/one".ToUri());
        }

        [Fact]
        public void fall_back_to_the_default_queue_if_no_segments()
        {
            "tcp://localhost:2222".ToUri().QueueName().ShouldBe(TransportConstants.Default);
        }

        [Fact]
        public void negative_case_with_loopback()
        {
            "loopback://default".ToUri().IsDurable().ShouldBeFalse();
            "loopback://replies".ToUri().IsDurable().ShouldBeFalse();
        }

        [Fact]
        public void negative_case_with_tcp()
        {
            "tcp://localhost:2200".ToUri().IsDurable().ShouldBeFalse();
            "tcp://localhost:2200/replies".ToUri().IsDurable().ShouldBeFalse();
        }

        [Fact]
        public void positive_case_with_loopback()
        {
            "loopback://durable".ToUri().IsDurable().ShouldBeTrue();
            "loopback://durable/replies".ToUri().IsDurable().ShouldBeTrue();
        }

        [Fact]
        public void positive_case_with_tcp()
        {
            "tcp://localhost:2200/durable".ToUri().IsDurable().ShouldBeTrue();
            "tcp://localhost:2200/durable/replies".ToUri().IsDurable().ShouldBeTrue();
        }

        [Fact]
        public void still_get_queue_name_with_durable()
        {
            "tcp://localhost:2222/durable".ToUri().QueueName().ShouldBe(TransportConstants.Default);
            "tcp://localhost:2222/durable/incoming".ToUri().QueueName().ShouldBe("incoming");
        }

        [Fact]
        public void use_the_last_segment_if_it_exists()
        {
            "tcp://localhost:2222/incoming".ToUri().QueueName().ShouldBe("incoming");
        }
    }
}
