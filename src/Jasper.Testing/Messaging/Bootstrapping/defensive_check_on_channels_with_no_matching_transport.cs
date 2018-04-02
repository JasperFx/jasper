using System.Threading.Tasks;
using Jasper.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Bootstrapping
{
    public class defensive_check_on_channels_with_no_matching_transport
    {
        [Fact]
        public async Task will_throw_an_exception()
        {
            var registry = new JasperRegistry();

            registry.Transports.ListenForMessagesFrom("foo://1");
            registry.Transports.ListenForMessagesFrom("foo://2");

            registry.Handlers.DisableConventionalDiscovery();



            var ex = await Testing.Exception<UnknownTransportException>.ShouldBeThrownByAsync(async () =>
            {
                var runtime = await JasperRuntime.ForAsync(registry);
                await runtime.Shutdown();
            });

            ex.Message.ShouldContain("foo://1");
            ex.Message.ShouldContain("foo://2");
        }
    }
}
