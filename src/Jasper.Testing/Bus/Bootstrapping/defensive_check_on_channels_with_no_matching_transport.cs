using Jasper.Bus;
using Jasper.Testing.Http;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class defensive_check_on_channels_with_no_matching_transport
    {
        [Fact]
        public void will_throw_an_exception()
        {
            var ex = Testing.Exception<UnknownTransportException>.ShouldBeThrownBy(() =>
            {
                var registry = new JasperRegistry();

                registry.Transports.ListenForMessagesFrom("foo://1");
                registry.Transports.ListenForMessagesFrom("foo://2");

                registry.Handlers.DisableConventionalDiscovery();

                using (var runtime = JasperRuntime.For(registry))
                {

                }
            });

            ex.Message.ShouldContain("foo://1");
            ex.Message.ShouldContain("foo://2");
        }
    }
}
