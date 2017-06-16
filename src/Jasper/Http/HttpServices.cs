using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public class HttpServices : ServiceRegistry
    {
        public HttpServices(RouteGraph routes)
        {
            this.AddService(routes);
            this.AddService<IUrlRegistry>(routes.Router.Urls);
        }
    }
}