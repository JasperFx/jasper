using System;
using TestingSupport;

namespace Jasper.Http.Testing.Routing
{
    public class RoutingApp : JasperOptions
    {
        public RoutingApp()
        {
            Handlers.DisableConventionalDiscovery();

            Extensions.ConfigureHttp(x =>
            {
                x.DisableConventionalDiscovery()
                    .IncludeType<SpreadHttpActions>()
                    .IncludeType<RouteEndpoints>();
            });
        }
    }
}
