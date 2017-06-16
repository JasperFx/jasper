using System;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.LightningQueues;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Transports.LightningQueues
{
    public class LightningUriTester
    {
        [Fact]
        public void translates_localhost_to_machine_name()
        {
            var uri = new LightningUri("lq.tcp://localhost:2200/foo");
            uri.Original.ShouldBe(new Uri("lq.tcp://localhost:2200/foo"));
            uri.Port.ShouldBe(2200);
            uri.QueueName.ShouldBe("foo");

            ShouldBeBooleanExtensions.ShouldBeTrue(uri.Address.Host.EqualsIgnoreCase(Environment.MachineName));
        }

        [Fact]
        public void translates_home_ip_to_machine_name()
        {
            var uri = new LightningUri("lq.tcp://127.0.0.1:2200/foo");
            ShouldBeBooleanExtensions.ShouldBeTrue(uri.Address.Host.EqualsIgnoreCase(Environment.MachineName));
        }

        [Fact]
        public void does_not_translate_remote_host()
        {
            var address = "lq.tcp://server1:2230/foo";
            var uri = new LightningUri(address);
            uri.Address.ShouldBe(address.ToUri());
            uri.Original.ShouldBe(address.ToUri());
        }

        [Fact]
        public void blows_up_if_protocol_is_not_lightning_queues()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                new LightningUri("foo://bar");
            });
        }
    }
}