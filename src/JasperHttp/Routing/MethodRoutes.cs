using System;
using System.Linq;
using JasperHttp.Model;
using JasperHttp.Routing.Codegen;
using Lamar;
using LamarCodeGeneration;

namespace JasperHttp.Routing
{
    public class MethodRoutes : RouteNode
    {
        private GeneratedType _generatedType;

        public string HttpMethod { get; }

        public MethodRoutes(string httpMethod) : base("", 0)
        {
            HttpMethod = httpMethod;
        }

        public Route Root { get; set; }

        public void AssemblySelector(GeneratedAssembly assembly, RouteGraph routes)
        {
            _generatedType = assembly.AddType(HttpMethod + "Router", typeof(RouteSelector));

            foreach (var route in routes.Where(x => x.RespondsToMethod(HttpMethod)))
            {
                route.WriteRouteMatchMethod(_generatedType);
            }

            var method = _generatedType.MethodFor(nameof(RouteSelector.Select));
            method.Frames.Add(new FindRouteFrame(this, routes));
        }

        public RouteSelector BuildSelector(IContainer container, RouteGraph routes)
        {
            var code = _generatedType.SourceCode;

            var selectorType = _generatedType.CompiledType;
            var selector = (RouteSelector)Activator.CreateInstance(selectorType);
            foreach (var route in routes.Where(x => x.RespondsToMethod(HttpMethod)))
            {
                var handler = route.CreateHandler(container);
                var setter = selectorType.GetProperty(route.Route.VariableName);
                setter?.SetValue(selector, handler);
            }

            return selector;
        }
    }
}
