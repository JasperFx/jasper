using Jasper.Bus.Delayed;
using Jasper.Bus.Transports.Loopback;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_delayed_job_processor : BootstrappingContext
    {
        [Fact]
        public void should_add_the_delayed_queue_if_using_in_memory_delayed_processor()
        {
            theChannels.HasChannel(LoopbackTransport.Delayed).ShouldBeTrue();

        }

        [Fact]
        public void default_is_to_use_in_memory_delayed_processor()
        {
            theRuntime.Container.DefaultSingletonIs<IDelayedJobProcessor, InMemoryDelayedJobProcessor>();
        }
    }
}
