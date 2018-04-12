using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Conneg;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Transport;
using Jasper.Messaging.Transports.Configuration;
using Lamar.Util;
using Newtonsoft.Json;

namespace Jasper.Http
{
    public partial class HttpSettings
    {
        private readonly HttpTransportSettings _transport;

        internal readonly RouteGraph Routes = new RouteGraph();

        public HttpSettings(MessagingSettings settings)
        {
            _methodFilters = new ActionMethodFilter();
            _methodFilters.Excludes += m => m.Name == "Configure";


            _transport = settings.Http;
            IncludeClassesSuffixedWithEndpoint();
        }

        /// <summary>
        ///     Completely enable or disable all Jasper HTTP features
        /// </summary>
        public bool Enabled { get; set; } = true;

        public JsonSerializerSettings JsonSerialization { get; set; } = new JsonSerializerSettings();

        public MediaSelectionMode MediaSelectionMode { get; set; } = MediaSelectionMode.All;


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

                    if (_transport.IsEnabled)
                    {
#pragma warning disable 4014
                        Routes.AddRoute<TransportEndpoint>(x => x.put__messages(null, null, null),
                            _transport.RelativeUrl).Route.HttpMethod = "PUT";


                        Routes.AddRoute<TransportEndpoint>(x => x.put__messages_durable(null, null, null),
                            _transport.RelativeUrl.AppendUrl("durable")).Route.HttpMethod = "PUT";

#pragma warning restore 4014
                    }
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
