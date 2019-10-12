using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Baseline;
using Baseline.Reflection;
using LamarCodeGeneration.Frames;

namespace JasperHttp.Model
{
    public class RouteGraph : IEnumerable<RouteChain>
    {
        public static readonly string Context = "httpContext";

        private readonly IList<RouteChain> _chains = new List<RouteChain>();

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
        ///     Union of routed chains that respond to GET or HEAD
        /// </summary>
        public IEnumerable<RouteChain> Resources => Gets.Union(Heads);

        /// <summary>
        ///     Union of routed chains that respond to POST, PUT, or DELETE
        /// </summary>
        public IEnumerable<RouteChain> Commands => Posts.Union(Puts).Union(Deletes);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RouteChain> GetEnumerator()
        {
            return _chains.GetEnumerator();
        }

        public RouteChain AddRoute(MethodCall methodCall)
        {
            var route = new RouteChain(methodCall);
            _chains.Add(route);

            return route;
        }

        public RouteChain ChainForAction<T>(Expression<Action<T>> expression)
        {
            var method = ReflectionHelper.GetMethod(expression);

            return _chains.FirstOrDefault(x => x.Action.HandlerType == typeof(T) && Equals(x.Action.Method, method));
        }


        public void AssertNoDuplicateRoutes()
        {
            var duplicates = _chains
                .GroupBy(x => x.Route.Name)
                .Where(x => x.Count() > 1)
                .Select(group => new DuplicateRoutesException(group)).ToArray();

            if (duplicates.Length == 1) throw duplicates[0];

            if (duplicates.Any()) throw new AggregateException(duplicates);
        }

        internal void Seal()
        {
            _sealed = true;
        }

        private bool _sealed;

        internal bool IsSealed()
        {
            return _sealed;
        }
    }

    public class DuplicateRoutesException : Exception
    {
        public DuplicateRoutesException(IEnumerable<RouteChain> chains) : base(
            $"Duplicated route with pattern {chains.First().Route.Name} between {chains.Select(x => $"{x.Action}").Join(", ")}")
        {
        }
    }
}
