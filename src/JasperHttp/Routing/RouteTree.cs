using System.Linq;
using Baseline;
using Jasper.Configuration;
using JasperHttp.ContentHandling;
using JasperHttp.Internal;
using JasperHttp.Model;
using JasperHttp.Routing.Codegen;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;
using LamarCompiler;
using Microsoft.AspNetCore.Http;

namespace JasperHttp.Routing
{
    public class RouteTree
    {
        public static string[] Empty = new string[0];

        private readonly LightweightCache<string, MethodRoutes> _methods
            = new LightweightCache<string, MethodRoutes>(method => new MethodRoutes(method));

        private readonly JasperHttpOptions _options;
        private readonly JasperGenerationRules _rules;
        private readonly GeneratedAssembly _assembly;

        private ImHashMap<string, RouteSelector> _selectors = ImHashMap<string, RouteSelector>.Empty;

        public RouteTree(JasperHttpOptions options, JasperGenerationRules rules)
        {
            foreach (var chain in options.Routes)
            {
                chain.Route.Place(this);
            }

            _options = options;
            _rules = rules;
            _assembly = new GeneratedAssembly(rules);
        }


        public MethodRoutes ForMethod(string httpMethod)
        {
            return _methods[httpMethod.ToUpper()];
        }

        public RouteHandler SelectRoute(HttpContext context, out string[] segments)
        {
            if (_selectors.TryFind(context.Request.Method.ToUpper(), out var selector))
            {
                return selector.Select(context, out segments);
            }

            segments = Empty;

            return null;
        }

        public void CompileAll(IContainer container)
        {
            var connegRules = container.GetInstance<ConnegRules>();

            foreach (var route in _options.Routes)
            {
                route.AssemblyType(_assembly, connegRules, _rules);
            }

            foreach (var methodRoutes in _methods)
            {
                methodRoutes.AssemblySelector(_assembly, _options.Routes);
            }

            new AssemblyGenerator().Compile(_assembly, container.CreateServiceVariableSource());

            foreach (var methodRoutes in _methods)
            {
                var selector = methodRoutes.BuildSelector(container, _options.Routes);
                selector.Root = methodRoutes.Root?.Handler;
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
