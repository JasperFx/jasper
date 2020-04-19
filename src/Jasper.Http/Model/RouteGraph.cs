using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using BaselineTypeDiscovery;
using Jasper.Http.ContentHandling;
using Lamar;
using LamarCodeGeneration;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace Jasper.Http.Model
{
    public class RouteGraph : IGeneratesCode, IEnumerable<RouteChain>
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

        internal IContainer Container { get; set; }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<RouteChain> GetEnumerator()
        {
            return _chains.GetEnumerator();
        }

        public IServiceVariableSource AssemblyTypes(GenerationRules rules, GeneratedAssembly assembly)
        {
            if (Container == null)
                throw new InvalidOperationException(
                    "The Container property needs to be set before this is a valid operation");

            // This has to be built by the container because of the
            // discovery of readers/writers/serializers
            var conneg = Container.GetInstance<ConnegRules>();

            foreach (var chain in _chains) chain.AssemblyType(assembly, conneg, rules, Container);

            return Container.CreateServiceVariableSource();
        }

        public async Task AttachPreBuiltTypes(GenerationRules rules, Assembly assembly, IServiceProvider services)
        {
            var typeSet = await TypeRepository.ForAssembly(assembly);
            var handlerTypes = typeSet.ClosedTypes.Concretes.Where(x => x.CanBeCastTo<RouteHandler>()).ToArray();

            var container = (IContainer) services;

            foreach (var chain in _chains) chain.AttachPreBuiltHandler(rules, container, handlerTypes);
        }

        public Task AttachGeneratedTypes(GenerationRules rules, IServiceProvider services)
        {
            var container = (IContainer) services;

            foreach (var chain in _chains) chain.CreateHandler(container);

            return Task.CompletedTask;
        }

        public string CodeType { get; } = "Routes";

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
    }
}
