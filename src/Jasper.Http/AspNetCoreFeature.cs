using System;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using BlueMilk.Codegen;
using BlueMilk.Util;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Http.Transport;
using Jasper.Util;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    public class AspNetCoreFeature : IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;

        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();


            _inner = new WebHostBuilder();
        }

        public IWebHost Host { get; private set; }

        internal string EnvironmentName
        {
            get => _inner.GetSetting(WebHostDefaults.EnvironmentKey);
            set => this.UseEnvironment(value);
        }


        internal bool BootstrappedWithinAspNetCore { get; set; }

        public HttpSettings Settings { get; } = new HttpSettings();

        public void Dispose()
        {
            Host?.Dispose();
        }


        internal Task FindRoutes(JasperRuntime runtime, JasperHttpRegistry registry, PerfTimer timer)
        {
            var applicationAssembly = registry.ApplicationAssembly;
            var generation = registry.CodeGeneration;


            _inner.UseSetting(WebHostDefaults.ApplicationKey, applicationAssembly.FullName);

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

                var rules = timer.Record("Fetching Conneg Rules", () =>
                {
                    return runtime.Container.QuickBuild<ConnegRules>();
                });

                timer.Record("Build Routing Tree", () =>
                {
                    Routes.BuildRoutingTree(rules, generation, runtime);
                });

                if (BootstrappedWithinAspNetCore) return;

                timer.Record("Activate ASP.Net", () =>
                {
                    activateLocally(runtime);
                });


            });
        }


        internal void Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer)
        {

        }

        public void Describe(JasperRuntime runtime, TextWriter writer)
        {
            if (runtime.HttpAddresses == null) return;
            foreach (var url in runtime.HttpAddresses.Split(';')) writer.WriteLine($"Now listening on: {url}");
        }

        IWebHost IWebHostBuilder.Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        IWebHostBuilder IWebHostBuilder.UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        string IWebHostBuilder.GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }

        public IWebHostBuilder ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            return _inner.ConfigureAppConfiguration(configureDelegate);
        }

        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        private void activateLocally(JasperRuntime runtime)
        {
            _inner.ConfigureServices(services => JasperStartup.Register(runtime.Container, services, Routes.Router));

            Host = _inner.Build();

            runtime.Container.Configure(x => x.AddSingleton(Host));

            Host.Start();
        }

        private readonly HttpTransportSettings _transport = new HttpTransportSettings();

        public IHttpTransportConfiguration Transport => _transport;
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


        public bool EnableMessageTransport { get; set; } = false;

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
