using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http.Routing.Codegen;
using Microsoft.AspNetCore.Http;

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
            var route = new Route(pattern, method, appfunc);

            Add(route);

            
        }

        public UrlGraph Urls { get; } = new UrlGraph();

        public void Add(Route route)
        {
            _trees[route.HttpMethod.ToUpperInvariant()].AddRoute(route);
            Urls.Register(route);
        }

        // TODO -- dunno that this needs to be done by verb. Reconsider
        public void AddNotFoundHandler(string method, RequestDelegate handler)
        {
            _trees[method.ToUpperInvariant()].NotFound = handler;
        }

        public Task Invoke(HttpContext context)
        {
            string[] segments;
            var route = SelectRoute(context, out segments);

            context.Items.Add(RoutingFrames.Segments, segments);

            // TODO -- add some error handling to 500 here. May also change how segments are being smuggled into the HttpContext
            if (route == null) return _trees[context.Request.Method].NotFound(context);

            context.Response.StatusCode = 200;

            context.SetSegments(segments);
            // TODO -- going to eliminate this.
            route.SetValues(context, segments);
            

            return route.Invoker(context);
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
