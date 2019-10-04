using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Lamar;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    internal class JasperHttpServiceRegistry : ServiceRegistry
    {
        public JasperHttpServiceRegistry(JasperHttpOptions options)
        {
            this.AddSingleton(options);
            ForConcreteType<ConnegRules>().Configure.Singleton();
            For<IHttpContextAccessor>().Use(x => new HttpContextAccessor());
            this.AddSingleton(options.Routes);

            ForSingletonOf<IUrlRegistry>().Use<UrlGraph>();


            Policies.Add(new RouteScopingPolicy(options.Routes));

        }
    }
}
