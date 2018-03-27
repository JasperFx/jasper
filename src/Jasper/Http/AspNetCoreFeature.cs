using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Transport;
using Lamar.Codegen;
using Lamar.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public class AspNetCoreFeature
    {
        private readonly HttpTransportSettings _transport = new HttpTransportSettings();

        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();
        }

        /// <summary>
        /// Completely enable or disable all Jasper HTTP features
        /// </summary>
        public bool Enabled { get; set; } = true;

        public HttpSettings Settings { get; } = new HttpSettings();

        public IHttpTransportConfiguration Transport => _transport;


        internal Task FindRoutes(JasperRuntime runtime, JasperRegistry registry, PerfTimer timer)
        {
            var applicationAssembly = registry.ApplicationAssembly;
            var generation = registry.CodeGeneration;

            return Actions.FindActions(applicationAssembly).ContinueWith(t =>
            {
                timer.Record("Find Routes", () =>
                {
                    var actions = t.Result;
                    foreach (var methodCall in actions) Routes.AddRoute(methodCall);

                    if (_transport.EnableMessageTransport)
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

                timer.Record("Build Routing Tree", () =>
                {
                    Routes.BuildRoutingTree(rules, generation, runtime);
                });

            });
        }


        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses) writer.WriteLine($"Now listening on: {url}");
        }

    }

    public interface IHttpTransportConfiguration
    {
        IHttpTransportConfiguration EnableListening(bool enabled);
        IHttpTransportConfiguration RelativeUrl(string url);
        IHttpTransportConfiguration ConnectionTimeout(TimeSpan span);
    }


    public class HttpTransportSettings : IHttpTransportConfiguration
    {
        public TimeSpan ConnectionTimeout { get; set; } = 10.Seconds();
        public string RelativeUrl { get; set; } = "messages";


        public bool EnableMessageTransport { get; set; }

        IHttpTransportConfiguration IHttpTransportConfiguration.EnableListening(bool enabled)
        {
            EnableMessageTransport = enabled;
            return this;
        }

        IHttpTransportConfiguration IHttpTransportConfiguration.RelativeUrl(string url)
        {
            RelativeUrl = url;
            return this;
        }

        IHttpTransportConfiguration IHttpTransportConfiguration.ConnectionTimeout(TimeSpan span)
        {
            ConnectionTimeout = span;
            return this;
        }
    }
}
