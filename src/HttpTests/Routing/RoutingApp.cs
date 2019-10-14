using Jasper;
using JasperHttp;
using TestingSupport;

namespace HttpTests.Routing
{
    public class RoutingApp : JasperRegistry
    {
        public RoutingApp()
        {
            Handlers.DisableConventionalDiscovery();

            Settings.Http(x =>
            {
                x.DisableConventionalDiscovery()
                    .IncludeType<SpreadHttpActions>()
                    .IncludeType<RouteEndpoints>();
            });
        }
    }
}
