using System;
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

namespace Jasper.Http
{
    // TODO -- this should be "loadable" from JSON too
    public partial class HttpSettings
    {
        internal readonly RouteGraph Routes = new RouteGraph();
        private Task _findActions;

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

            return _findActions.ContinueWith(t => { Routes.BuildRoutingTree(generation, container); });
        }


        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses) writer.WriteLine($"Now listening on: {url}");
        }
    }
}
