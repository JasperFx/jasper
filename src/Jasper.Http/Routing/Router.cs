using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.Model;
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
            HttpVerbs.All.Each(x => _trees.Add(x, new RouteTree(x)));
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

            if (route == null)
            {
                await next(context);
            }
            else
            {
                context.Response.StatusCode = 200;

                context.SetSegments(segments);

                try
                {
                    if (route.Handler == null)
                    {
                        lock (route)
                        {
                            if (route.Handler == null)
                            {
                                route.Handler = HandlerBuilder.Build(route.Chain);
                            }
                        }
                    }

                    await route.Handler.Handle(context);
                }
                catch (Exception e)
                {
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
        public RouteHandlerBuilder HandlerBuilder { get; set; }
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
