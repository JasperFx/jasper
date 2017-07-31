using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using StructureMap;

namespace Jasper.Http
{
    public class AspNetCoreFeature : IFeature
    {
        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        private readonly HostBuilder _builder;
        private readonly Registry _services;
        private IWebHost _host;

        public AspNetCoreFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();

            _services = new Registry();
            _builder = new HostBuilder(this);
        }

        public string EnvironmentName
        {
            get => _builder.GetSetting(WebHostDefaults.EnvironmentKey);
            set => _builder.UseEnvironment(value);
        }


        public IWebHostBuilder WebHostBuilder => _builder;

        public void Dispose()
        {
            _host?.Dispose();
        }

        [Obsolete("Not gonna be necessary any more")]
        public HostingConfiguration Hosting { get; } = new HostingConfiguration();

        public async Task<Registry> Bootstrap(JasperRegistry registry)
        {
            var actions = await Actions.FindActions(registry.ApplicationAssembly);
            foreach (var methodCall in actions)
            {
                Routes.AddRoute(methodCall);
            }

            _services.For<RouteGraph>().Use(Routes);
            _services.For<IUrlRegistry>().Use(Routes.Router.Urls);

            _services.For<IServer>().Add<NulloServer>().Singleton();

            return _services;
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                var rules = runtime.Get<ConnegRules>();

                Routes.BuildRoutingTree(rules, generation, runtime.Container);

                _host = _builder.Activate(runtime.Container);

                runtime.Container.Inject(_host);

                _host.Start();
            });
        }
    }

    public class HostingConfiguration
    {
        public bool UseKestrel { get; set; } = true;
        public bool UseIIS { get; set; } = true;
        public int Port { get; set; } = 3000;
        public string ContentRoot { get; set; } = Directory.GetCurrentDirectory();
    }

    internal class NulloServer : IServer
    {
        public void Dispose()
        {

        }

        public void Start<TContext>(IHttpApplication<TContext> application)
        {

        }

        public IFeatureCollection Features { get; } = new FeatureCollection();
    }
}
