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
                var registry = new JasperBusRegistry();
                registry.Channel("foo://1");
                registry.Channel("foo://2");
                registry.Handlers.ExcludeTypes(x => true);

                JasperRuntime.For(registry);
            });

            ex.Message.ShouldContain("foo://1");
            ex.Message.ShouldContain("foo://2");
        }
    }
}
