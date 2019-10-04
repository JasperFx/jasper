using Jasper;
using TestingSupport;

namespace HttpTests.Routing
{
    public class RoutingApp : JasperRegistry
    {
        public RoutingApp()
        {
            Handlers.DisableConventionalDiscovery();
            JasperHttpRoutes
                .DisableConventionalDiscovery()
                .IncludeType<SpreadHttpActions>()
                .IncludeType<RouteEndpoints>();
        }
    }
}
