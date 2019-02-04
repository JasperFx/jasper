using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing.Codegen;
using Jasper.Util;
using Lamar;
using LamarCompiler;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using Microsoft.AspNetCore.Http;

namespace Jasper.Http.Routing
{
    public class RouteTree
    {
        public static string[] Empty = new string[0];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string[] ToSegments(string route)
        {
            if (route == "/") return Empty;

            return route.TrimStart('/').TrimEnd('/').Split('/');
        }


        private readonly LightweightCache<string, MethodRoutes> _methods
            = new LightweightCache<string, MethodRoutes>(method => new MethodRoutes(method));

        private readonly HttpSettings _settings;
        private readonly JasperGenerationRules _rules;
        private readonly GeneratedAssembly _assembly;

        private ImHashMap<string, RouteSelector> _selectors = ImHashMap<string, RouteSelector>.Empty;

        public RouteTree(HttpSettings settings, JasperGenerationRules rules)
        {
            foreach (var chain in settings.Routes)
            {
                chain.Route.Place(this);
            }

            _settings = settings;
            _rules = rules;
            _assembly = new GeneratedAssembly(rules);
        }


        public MethodRoutes ForMethod(string httpMethod)
        {
            return _methods[httpMethod.ToUpper()];
        }

        public RouteHandler SelectRoute(HttpContext context, out string[] segments)
        {
            segments = ToSegments(context.Request.Path);


            return _selectors.TryFind(context.Request.Method.ToUpper(), out var selector)
                ? selector.Select(segments)
                : null;
        }

        public void CompileAll(IContainer container)
        {
            var connegRules = container.GetInstance<ConnegRules>();

            foreach (var route in _settings.Routes)
            {
                route.AssemblyType(_assembly, connegRules, _rules);
            }

            foreach (var methodRoutes in _methods)
            {
                methodRoutes.AssemblySelector(_assembly, _settings.Routes);
            }

            container.CompileWithInlineServices(_assembly);

            foreach (var methodRoutes in _methods)
            {
                var selector = methodRoutes.BuildSelector(container, _settings.Routes);
                _selectors = _selectors.AddOrUpdate(methodRoutes.HttpMethod, selector);
            }
        }


    }

    public class FindRouteFrame : SyncFrame
    {
        private readonly MethodRoutes _routes;

        public FindRouteFrame(MethodRoutes routes, RouteGraph graph)
        {
            _routes = routes;


            graph
                .Where(x => x.RespondsToMethod(routes.HttpMethod))
                .Select(x => new Setter(typeof(RouteHandler), x.Route.VariableName){InitialValue = x.Route.Handler})
                .Each(x => uses.Add(x));
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            _routes.WriteSelectCode(writer);

            writer.ReturnNull();
        }
    }
}
