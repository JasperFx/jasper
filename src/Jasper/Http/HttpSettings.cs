using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Lamar;
using Newtonsoft.Json;
using Polly;

namespace Jasper.Http
{
    public partial class HttpSettings
    {
        internal readonly RouteGraph Routes = new RouteGraph();
        private Task _findActions;

        private readonly IList<IRoutePolicy> _policies = new List<IRoutePolicy>();

        public HttpSettings()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.Name == "Configure";

            MethodFilters.Excludes += m => m.DeclaringType == typeof(object);
            MethodFilters.Excludes += m => m.HasAttribute<JasperIgnoreAttribute>();
            MethodFilters.Excludes += m => m.DeclaringType.HasAttribute<JasperIgnoreAttribute>();

            MethodFilters.Includes += m => m.Name.EqualsIgnoreCase("Index");

            MethodFilters.Includes += m =>
            {
                return HttpVerbs.All.Contains(m.Name, StringComparer.OrdinalIgnoreCase) ||
                       HttpVerbs.All.Any(x => m.Name.StartsWith(x + "_", StringComparison.OrdinalIgnoreCase));
            };


            IncludeClassesSuffixedWithEndpoint();
        }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void GlobalPolicy<T>() where T : IRoutePolicy, new()
        {
            GlobalPolicy(new T());
        }

        /// <summary>
        ///     Applies a handler policy to all known message handlers
        /// </summary>
        /// <param name="policy"></param>
        public void GlobalPolicy(IRoutePolicy policy)
        {
            _policies.Add(policy);
        }

        /// <summary>
        ///     Completely enable or disable all Jasper HTTP features
        /// </summary>
        public bool Enabled { get; set; } = true;

        // Call this in UseJasper()
        internal void StartFindingRoutes(Assembly assembly)
        {
            if (!Enabled) return;

            _findActions = FindActions(assembly).ContinueWith(t =>
            {
                var actions = t.Result;
                foreach (var methodCall in actions) Routes.AddRoute(methodCall);
            });
        }

        // Call this from the activator
        internal Task BuildRouting(IContainer container, JasperGenerationRules generation)
        {
            if (!Enabled) return Task.CompletedTask;

            return _findActions.ContinueWith(t =>
            {
                foreach (var policy in _policies)
                {
                    policy.Apply(Routes, generation);
                }


                Routes.BuildRoutingTree(generation, container);
            });
        }


        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses) writer.WriteLine($"Now listening on: {url}");
        }
    }

    public interface IRoutePatternStrategy
    {
        bool Matches(MethodInfo method);

    }
}
