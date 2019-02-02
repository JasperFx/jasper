using Jasper;
using TestingSupport;

namespace HttpTests.Routing
{
    public class RoutingApp : JasperRegistry
    {
        public RoutingApp()
        {
            Handlers.DisableConventionalDiscovery();
            HttpRoutes
                .DisableConventionalDiscovery()
                .IncludeType<SpreadHttpActions>()
                .IncludeType<RouteEndpoints>();
        }
    }
}
