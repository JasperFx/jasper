namespace Jasper.Testing.Http.Routing
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
