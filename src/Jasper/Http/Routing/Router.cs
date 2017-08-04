using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.Routing.Codegen;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Http.Routing
{
    public class Router
    {
        private readonly IDictionary<string, RouteTree> _trees = new Dictionary<string, RouteTree>();

        public Router()
        {
            HttpVerbs.All.Each(x => _trees.Add(x, new RouteTree()));
        }

        public void Add(string method, string pattern,RequestDelegate appfunc)
        {
            var route = new Route(pattern, method);

            Add(route);
        }

        public RequestDelegate NotFound { get; set; } = c =>
        {
            c.Response.StatusCode = 404;
            c.Response.Headers["status-description"] = "Resource Not Found";
            return c.Response.WriteAsync("Resource Not Found");
        };

        public UrlGraph Urls { get; } = new UrlGraph();

        public void Add(Route route)
        {
            _trees[route.HttpMethod.ToUpperInvariant()].AddRoute(route);
            Urls.Register(route);
        }

        public Task Invoke(HttpContext context)
        {
            return Invoke(context, NotFound);
        }

        public RequestDelegate Apply(RequestDelegate @delegate)
        {
            NotFound = @delegate;
            return Invoke;
        }

        public async Task Invoke(HttpContext context, RequestDelegate next)
        {
            string[] segments;
            var route = SelectRoute(context, out segments);

            // TODO -- add some error handling to 500 here. May also change how segments are being smuggled into the HttpContext
            if (route == null)
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = 200;

                context.SetSegments(segments);


                // TODO -- going to eliminate this.
                route.SetValues(context, segments);

                try
                {
                    await route.Handler.Handle(context);
                }
                catch (Exception e)
                {
                    // TODO -- do something fancier here
                    context.RequestServices.GetService<ILogger<HttpContext>>().LogError(new EventId(500), e, "Request Failed");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync(e.ToString());
                }
            }
        }

        public Route SelectRoute(HttpContext context, out string[] segments)
        {
            var method = context.Request.Method;
            var routeTree = _trees[method];

            segments = RouteTree.ToSegments(context.Request.Path);
            return routeTree.Select(segments);
        }

        public RouteAdder Get => new RouteAdder(HttpVerbs.GET, this);
        public RouteAdder Post => new RouteAdder(HttpVerbs.POST, this);
        public RouteAdder Put => new RouteAdder(HttpVerbs.PUT, this);
        public RouteAdder Delete => new RouteAdder(HttpVerbs.DELETE, this);
        public RouteAdder Head => new RouteAdder(HttpVerbs.HEAD, this);
        public RouteAdder Options => new RouteAdder(HttpVerbs.OPTIONS, this);
        public RouteAdder Patch => new RouteAdder(HttpVerbs.PATCH, this);
    }

    public class RouteAdder
    {
        private readonly string _httpVerb;
        private readonly Router _parent;

        public RouteAdder(string httpVerb, Router parent)
        {
            _httpVerb = httpVerb;
            _parent = parent;
        }

        public RequestDelegate this[string pattern]
        {
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(value));

                _parent.Add(_httpVerb, pattern, value);
            }
        }
    }
}
