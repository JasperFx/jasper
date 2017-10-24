using Jasper.Bus;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Durable;
using Jasper.Bus.Transports.Lightweight;
using Jasper.Bus.Transports.Loopback;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class JasperRegistry_Transports_configuration : BootstrappingContext
    {
        [Fact]
        public void set_the_port_of_lightweight()
        {
            theRegistry.Transports.Lightweight.ListenOnPort(2222);

            theSettings.Lightweight.Port.ShouldBe(2222);
        }

        [Fact]
        public void set_the_port_of_durable()
        {
            theRegistry.Transports.Durable.ListenOnPort(2233);

            theSettings.Durable.Port.ShouldBe(2233);
        }

        [Fact]
        public void disable_lightweight()
        {
            theRegistry.Transports.Lightweight.Disable();

            theSettings.Lightweight.State.ShouldBe(TransportState.Disabled);
        }

        [Fact]
        public void set_the_maxiumum_send_attempts()
        {
            theRegistry.Transports.Durable.MaximumSendAttempts(3);

            theSettings.Durable.MaximumSendAttempts.ShouldBe(3);
        }

        [Fact]
        public void set_the_parallelization_for_the_default_queue()
        {
            theRegistry.Transports.Loopback.DefaultQueue.MaximumParallelization(2);

            theSettings.Loopback.DefaultQueue.Parallelization.ShouldBe(2);
        }

        [Fact]
        public void set_the_parallelization_for_another_queue()
        {
            theRegistry.Transports.Loopback.Queue("incoming").MaximumParallelization(10);

            theSettings.Loopback.Queues["incoming"].Parallelization.ShouldBe(10);
        }

        [Fact]
        public void make_a_queue_be_single_threaded()
        {
            theRegistry.Transports.Loopback.Queue("incoming").Sequential();

            theSettings.Loopback.Queues["incoming"].Parallelization.ShouldBe(1);
        }

        [Fact]
        public void disabled_transports_are_not_available()
        {
            theRegistry.Transports.Lightweight.Disable();

            var validTransports = theRuntime.Get<IChannelGraph>().ValidTransports;
            validTransports.ShouldNotContain(LightweightTransport.ProtocolName);
            validTransports.ShouldContain(LoopbackTransport.ProtocolName);
            validTransports.ShouldContain(DurableTransport.ProtocolName);
        }
    }
}
