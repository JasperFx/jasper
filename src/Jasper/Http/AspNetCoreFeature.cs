using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk;
using BlueMilk.Codegen;
using BlueMilk.IoC.Instances;
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
using Microsoft.Extensions.Logging;

namespace Jasper.Http
{
    public class AspNetCoreFeature : IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;

        private readonly ServiceRegistry _services;

        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();

            _services = new ServiceRegistry();
            _services.AddSingleton(Routes.Router);
            _services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            _services.AddSingleton<ConnegRules>();
            _services.AddSingleton<IServer, NulloServer>();

            _services.AddSingleton(x => x.GetService<JasperRuntime>().Host);

            _services.Policies.OnMissingFamily<LoggerPolicy>();

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


        internal Task FindRoutes(JasperRegistry registry, PerfTimer timer)
        {
            return Actions.FindActions(registry.ApplicationAssembly).ContinueWith(t =>
            {
                timer.Record("Find Routes", () =>
                {
                    var actions = t.Result;
                    foreach (var methodCall in actions) Routes.AddRoute(methodCall);

                    var httpTransportSettings = registry.BusSettings.Http;
                    if (httpTransportSettings.EnableMessageTransport)
                    {
#pragma warning disable 4014
                        Routes.AddRoute<TransportEndpoint>(x => x.put__messages(null, null, null),
                            httpTransportSettings.RelativeUrl).Route.HttpMethod = "PUT";


                        Routes.AddRoute<TransportEndpoint>(x => x.put__messages_durable(null, null, null),
                            httpTransportSettings.RelativeUrl.AppendUrl("durable")).Route.HttpMethod = "PUT";

#pragma warning restore 4014
                    }

                    _services.AddSingleton(Routes);
                    _services.AddSingleton<IUrlRegistry>(Routes.Router.Urls);
                });
            });
        }


        internal void Activate(JasperRuntime runtime, GenerationRules generation, PerfTimer timer)
        {
            timer.Record("AspNetCoreFeature.Activate", () =>
            {
                timer.Record("Build Routing Tree", () =>
                {
                    var rules = runtime.Container.QuickBuild<ConnegRules>();

                    Routes.BuildRoutingTree(rules, generation, runtime);
                });


                if (BootstrappedWithinAspNetCore) return;

                timer.Record("Activate ASP.Net", () =>
                {
                    activateLocally(runtime);
                });
            });
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

            Host.Start();
        }
    }

    public class LoggerPolicy : IFamilyPolicy
    {
        public ServiceFamily Build(Type type, ServiceGraph serviceGraph)
        {
            if (!type.Closes(typeof(ILogger<>)))
            {
                return null;
            }

            var inner = type.GetGenericArguments().Single();
            var loggerType = typeof(Logger<>).MakeGenericType(inner);

            Instance instance = null;
            if (inner.IsPublic)
            {
                instance = new ConstructorInstance(type, loggerType,
                    ServiceLifetime.Transient);
            }
            else
            {
                instance = new LambdaInstance(type, provider =>
                {
                    var factory = provider.GetService<ILoggerFactory>();
                    return Activator.CreateInstance(loggerType, factory);

                }, ServiceLifetime.Transient);
            }

            return new ServiceFamily(type, instance);
        }
    }
}
