using System.IO;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Messaging.Transports.Configuration;
using Lamar.Util;
using Newtonsoft.Json;

namespace Jasper.Http
{
    // TODO -- this should be "loadable" from JSON too
    public partial class HttpSettings
    {
        internal readonly RouteGraph Routes = new RouteGraph();

        public HttpSettings()
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.Name == "Configure";


            IncludeClassesSuffixedWithEndpoint();
        }

        /// <summary>
        ///     Completely enable or disable all Jasper HTTP features
        /// </summary>
        public bool Enabled { get; set; } = true;

        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings();


        internal Task FindRoutes(JasperRuntime runtime, JasperRegistry registry, PerfTimer timer)
        {
            var applicationAssembly = registry.ApplicationAssembly;
            var generation = registry.CodeGeneration;

            return FindActions(applicationAssembly).ContinueWith(t =>
            {
                timer.Record("Find Routes", () =>
                {
                    var actions = t.Result;
                    foreach (var methodCall in actions) Routes.AddRoute(methodCall);

                });

                var rules = timer.Record("Fetching Conneg Rules",
                    () => runtime.Container.QuickBuild<ConnegRules>());

                timer.Record("Build Routing Tree", () => { Routes.BuildRoutingTree(rules, generation, runtime); });
            });
        }


        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses) writer.WriteLine($"Now listening on: {url}");
        }
    }
}
