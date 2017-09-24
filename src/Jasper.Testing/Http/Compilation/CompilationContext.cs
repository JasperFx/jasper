using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline.Reflection;
using BlueMilk.Codegen;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Util.StructureMap;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Http.Compilation
{
    [Collection("compilation")]
    public abstract class CompilationContext<T>
    {
        private Lazy<IContainer> _container;
        public readonly ServiceRegistry services = new ServiceRegistry();

        protected Lazy<Dictionary<string, RouteHandler>> _routes;

        private readonly Lazy<string> _code;
        protected StubHttpContext theContext = StubHttpContext.Empty();

        protected Lazy<RouteGraph> _graph;
        private GenerationConfig config;

        public CompilationContext()
        {
            _container = new Lazy<IContainer>(() => new Container(services));


            _graph = new Lazy<RouteGraph>(() =>
            {
                config = new GenerationConfig("Jasper.Testing.Codegen.Generated");
                var container = _container.Value;
                config.Sources.Add(new StructureMapServices(container));

                config.Assemblies.Add(typeof(IContainer).GetTypeInfo().Assembly);
                config.Assemblies.Add(GetType().GetTypeInfo().Assembly);


                var graph = new RouteGraph();

                var methods = typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(x => x.DeclaringType != typeof(object) && x != null && !x.IsSpecialName);

                foreach (var method in methods)
                {
                    graph.AddRoute(typeof(T), method);
                }

                return graph;
            });



            _code = new Lazy<string>(() => Graph.GenerateCode(config));

            _routes = new Lazy<Dictionary<string, RouteHandler>>(() =>
            {
                var routers = Graph.CompileAndBuildAll(config, _container.Value.GetInstance);
                var dict = new Dictionary<string, RouteHandler>();
                foreach (var router in routers)
                {
                    dict.Add(router.Chain.Action.Method.Name, router);
                }

                return dict;
            });
        }

        public RouteGraph Graph => _graph.Value;


        public string theCode => _code.Value;

        public void AllRoutesCompileSuccessfully()
        {
            ShouldBeTestExtensions.ShouldBeGreaterThan(_routes.Value.Count, 0);
        }


        public Task Execute(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            var route = _routes.Value[method.Name];

            return route.Handle(theContext);
        }

    }
}
