using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JasperHttp.Routing
{
    public class Router
    {
        private readonly RouteTree _tree;

        public Router(RouteTree tree)
        {
            _tree = tree;
        }

        public Task Invoke(HttpContext context, RequestDelegate next)
        {
            var handler = _tree.SelectRoute(context, out var segments);

            if (handler == null) return next(context);

            return handler.Handle(context, segments);
        }

        public RequestDelegate Apply(RequestDelegate next)
        {
            return context => Invoke(context, next);
        }

        public static IApplicationBuilder
            BuildOut(IApplicationBuilder app)
        {
            var container = (IContainer) app.ApplicationServices;
            var rules = container.GetInstance<JasperGenerationRules>();

            var tree = container.GetInstance<JasperHttpOptions>()
                .BuildRouting(container, rules).GetAwaiter().GetResult();

            var router = new Router(tree);

            app.MarkJasperHasBeenApplied();

            return app.Use(router.Apply);


        }
    }
}
