using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class parallelization_bootstrapping : BootstrappingContext
    {
        [Fact]
        public void the_default_parallelization_is_5()
        {
            theRegistry.Channels.Add("loopback://1");

            theChannels["loopback://1"].MaximumParallelization.ShouldBe(5);
        }

        [Fact]
        public void control_channel_is_always_forced_to_be_single_threaded()
        {
            theRegistry.Channels.Add("loopback://1");
            theRegistry.Channels.Add("loopback://control").UseAsControlChannel();

            theChannels["loopback://1"].MaximumParallelization.ShouldBe(5);
            theChannels["loopback://control"].MaximumParallelization.ShouldBe(1);
        }

        [Fact]
        public void explicitly_configure_parallelization()
        {
            theRegistry.Channels.Add("loopback://1").MaximumParallelization(3);
        }
    }
}
