using System.Threading.Tasks;
using Jasper.Http.Model;
using Jasper.Http.Transport;
using Jasper.Testing;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.Transport
{
    public class endpoint_and_url_configuration
    {
        [Fact]
        public async Task transport_endpoints_are_not_enabled_by_default()
        {
            var runtime = await JasperRuntime.ForAsync(_ => _.Handlers.DisableConventionalDiscovery());

            try
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .ShouldBeNull();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .ShouldBeNull();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task transport_endpoints_are_enabled_and_a_chain_should_be_present()
        {
            var runtime = await JasperRuntime.ForAsync<JasperRegistry>(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();

                _.Transports.Http.EnableListening(true);

            });

            try
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .ShouldNotBeNull();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .ShouldNotBeNull();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task transport_endpoints_are_enabled_and_a_chain_should_be_present_with_default_urls()
        {
            var runtime = await JasperRuntime.ForAsync<JasperRegistry>(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();
                _.Transports.Http.EnableListening(true);
            });

            try
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .Route.Pattern.ShouldBe("messages");

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .Route.Pattern.ShouldBe("messages/durable");
            }
            finally
            {
                await runtime.Shutdown();
            }

        }

        [Fact]
        public async Task transport_endpoints_are_enabled_and_a_chain_should_be_present_with_overridden_urls()
        {
            var runtime = await JasperRuntime.ForAsync<JasperRegistry>(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();

                _.Transports.Http.EnableListening(true).RelativeUrl("api");
            });

            try
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .Route.Pattern.ShouldBe("api");

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .Route.Pattern.ShouldBe("api/durable");
            }
            finally
            {
                await runtime.Shutdown();
            }

        }
    }
}
