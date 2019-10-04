using System;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Http.Routing.Codegen;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Jasper.Http.Routing
{
    public class Router
    {
        private readonly ILogger<JasperHttpOptions> _logger;
        private readonly Lazy<RouteTree> _finder;

        public Router(Task<RouteTree> source, ILogger<JasperHttpOptions> logger)
        {
            _logger = logger;
            _finder = new Lazy<RouteTree>(() => source.GetAwaiter().GetResult());
        }

        public Task Invoke(HttpContext context, RequestDelegate next)
        {
            var handler = _finder.Value.SelectRoute(context, out var segments);

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

            var finderSource = container.GetInstance<JasperHttpOptions>()
                .BuildRouting(container, rules);

            var router = new Router(finderSource, container.GetInstance<ILogger<JasperHttpOptions>>());

            app.MarkJasperHasBeenApplied();

            return app.Use(router.Apply);


        }
    }
}
