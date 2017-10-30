using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Jasper.Http.ContentHandling;
using Jasper.Http.Routing;
using Jasper.Internals.Codegen;
using Jasper.Internals.Util;
using StructureMap;
using GenericEnumerableExtensions = Baseline.GenericEnumerableExtensions;

namespace Jasper.Http.Model
{
    public class RouteGraph : HandlerSet<RouteChain, RouteHandler>, IEnumerable<RouteChain>
    {
        public static readonly string Context = "httpContext";
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

        public RouteChain ChainForAction<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return _chains.FirstOrDefault(x => x.Action.HandlerType == typeof(T) && Equals(x.Action.Method, method));
        }

        public void BuildRoutingTree(ConnegRules rules, GenerationRules generation, JasperRuntime runtime)
        {
            assertNoDuplicateRoutes();

            foreach (var chain in _chains)
            {
                rules.Apply(chain);
            }

            var handlers = CompileAndBuildAll(generation, runtime.Get);

            foreach (var handler in handlers)
            {
                var route = handler.Chain.Route;
                route.Handler = handler;
                Router.Add(route);
            }
        }



        private void assertNoDuplicateRoutes()
        {
            var duplicates = _chains
                .GroupBy(x => x.Route.Name)
                .Where(x => x.Count() > 1)
                .Select(group => new DuplicateRoutesException(@group)).ToArray();

            if (duplicates.Length == 1)
            {
                throw duplicates[0];
            }

            if (duplicates.Any())
            {
                throw new AggregateException(duplicates);
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

        public IEnumerable<RouteChain> Gets
        {
            get { return this.Where(x => x.RespondsToMethod("GET")); }
        }


        public IEnumerable<RouteChain> Posts
        {
            get { return this.Where(x => x.RespondsToMethod("POST")); }
        }


        public IEnumerable<RouteChain> Puts
        {
            get { return this.Where(x => x.RespondsToMethod("PUT")); }
        }


        public IEnumerable<RouteChain> Deletes
        {
            get { return this.Where(x => x.RespondsToMethod("DELETE")); }
        }


        public IEnumerable<RouteChain> Heads
        {
            get { return this.Where(x => x.RespondsToMethod("HEAD")); }
        }

        /// <summary>
        /// Union of routed chains that respond to GET or HEAD
        /// </summary>
        public IEnumerable<RouteChain> Resources => Gets.Union(Heads);

        /// <summary>
        /// Union of routed chains that respond to POST, PUT, or DELETE
        /// </summary>
        public IEnumerable<RouteChain> Commands => Posts.Union(Puts).Union(Deletes);
    }

    public class DuplicateRoutesException : Exception
    {
        public DuplicateRoutesException(IEnumerable<RouteChain> chains) : base($"Duplicated route with pattern {chains.First().Route.Name} between {GenericEnumerableExtensions.Join(chains.Select(x => $"{x.Action}"), ", ")}")
        {
        }
    }
}
