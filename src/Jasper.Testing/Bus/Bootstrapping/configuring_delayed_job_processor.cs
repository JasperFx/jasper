using Jasper.Bus.Delayed;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_delayed_job_processor : BootstrappingContext
    {
        [Fact]
        public void should_add_the_delayed_queue_if_using_in_memory_delayed_processor()
        {
            theRegistry.DelayedJobs.RunInMemory();

            theChannels.HasChannel(InMemoryDelayedJobProcessor.Queue).ShouldBeTrue();

            var channel = theChannels[InMemoryDelayedJobProcessor.Queue];
            channel.Incoming.ShouldBeTrue();
            channel.Sender.ShouldNotBeNull();
        }

        [Fact]
        public void default_is_to_use_in_memory_delayed_processor()
        {
            theRuntime.Container.DefaultSingletonIs<IDelayedJobProcessor, InMemoryDelayedJobProcessor>();
        }
    }
}
