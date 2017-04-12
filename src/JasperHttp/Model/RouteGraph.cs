using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Jasper.Codegen;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace JasperHttp.Model
{
    public class RouteGraph : HandlerSet<RouteChain, HttpContext, RouteHandler>, IEnumerable<RouteChain>
    {
        public static readonly string Context = "context";
        public readonly Router Router = new Router();

        private readonly IList<RouteChain> _chains = new List<RouteChain>();

        protected override RouteChain[] chains => _chains.ToArray();


        public void AddRoute(Type handlerType, MethodInfo method)
        {
            var methodCall = new MethodCall(handlerType, method);
            AddRoute(methodCall);
        }

        public void AddRoute(MethodCall methodCall)
        {
            var route = new RouteChain(methodCall);
            _chains.Add(route);
        }

        public void BuildRoutingTree(IGenerationConfig generation, IContainer container)
        {
            var handlers = CompileAndBuildAll(generation, container);
            
            foreach (var handler in handlers)
            {
                var route = handler.Chain.Route;
                route.Handler = handler;
                Router.Add(route);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RouteChain> GetEnumerator()
        {
            return _chains.GetEnumerator();
        }
    }
}
