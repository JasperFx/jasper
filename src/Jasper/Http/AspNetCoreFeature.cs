using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace Jasper.Http
{


    public class AspNetCoreFeature : IFeature, IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;

        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        private readonly Registry _services;
        private IWebHost _host;

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();

            _services = new Registry();
            _services.For<Router>().Use(Routes.Router);
            _services.For<IHttpContextAccessor>().Use<HttpContextAccessor>().ContainerScoped();

            _inner = new WebHostBuilder();
        }

        internal string EnvironmentName
        {
            get => _inner.GetSetting(WebHostDefaults.EnvironmentKey);
            set => this.UseEnvironment(value);
        }


        internal bool BootstrappedWithinAspNetCore { get; set; }

        public void Dispose()
        {
            _host?.Dispose();
        }

        async Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            var actions = await Actions.FindActions(registry.ApplicationAssembly);
            foreach (var methodCall in actions)
            {
                Routes.AddRoute(methodCall);
            }

            _services.For<RouteGraph>().Use(Routes);
            _services.For<IUrlRegistry>().Use(Routes.Router.Urls);

            if (!BootstrappedWithinAspNetCore)
            {
                _services.For<IServer>().Add<NulloServer>().Singleton();
            }

            return _services;
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                var rules = runtime.Get<ConnegRules>();

                Routes.BuildRoutingTree(rules, generation, runtime.Container);

                if (BootstrappedWithinAspNetCore) return;

                activateLocally(runtime);
            });
        }

        private void activateLocally(JasperRuntime runtime)
        {
            _inner.ConfigureServices(services => JasperStartup.Register(runtime.Container, services, Routes.Router));

            _host = _inner.Build();

            runtime.Container.Inject(_host);

            _host.Start();
        }

        IWebHost IWebHostBuilder.Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        IWebHostBuilder IWebHostBuilder.UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            return _inner.UseLoggerFactory(loggerFactory);
        }

        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        IWebHostBuilder IWebHostBuilder.ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            return _inner.ConfigureLogging(configureLogging);
        }

        IWebHostBuilder IWebHostBuilder.UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        string IWebHostBuilder.GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }
    }

}
