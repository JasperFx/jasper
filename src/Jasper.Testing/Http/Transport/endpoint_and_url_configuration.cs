using Jasper.Bus.Transports.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Transport;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.Transport
{
    public class endpoint_and_url_configuration
    {
        [Fact]
        public void transport_endpoints_are_not_enabled_by_default()
        {
            using (var runtime = JasperRuntime.For(_ => _.Handlers.DisableConventionalDiscovery()))
            {

                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .ShouldBeNull();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .ShouldBeNull();
            }
        }

        [Fact]
        public void transport_endpoints_are_enabled_and_a_chain_should_be_present()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();

                _.Settings.Alter<BusSettings>(x => x.Http.EnableMessageTransport = true);
            }))
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .ShouldNotBeNull();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .ShouldNotBeNull();
            }
        }

        [Fact]
        public void transport_endpoints_are_enabled_and_a_chain_should_be_present_with_default_urls()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();
                _.Settings.Alter<BusSettings>(x => x.Http.EnableMessageTransport = true);
            }))
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .Route.Pattern.ShouldBe("messages");

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .Route.Pattern.ShouldBe("messages/durable");
            }
        }

        [Fact]
        public void transport_endpoints_are_enabled_and_a_chain_should_be_present_with_overridden_urls()
        {
            using (var runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery();
                _.Handlers.IncludeType<TransportEndpoint>();

                _.Settings.Alter<BusSettings>(x =>
                {

                    x.Http.EnableMessageTransport = true;
                    x.Http.RelativeUrl = "api";
                });
            }))
            {
                var routes = runtime.Get<RouteGraph>();

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages(null, null, null))
                    .Route.Pattern.ShouldBe("api");

                routes.ChainForAction<TransportEndpoint>(x => x.put__messages_durable(null, null, null))
                    .Route.Pattern.ShouldBe("api/durable");
            }
        }
    }
}
