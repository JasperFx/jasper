using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http.Configuration;
using Jasper.Http.ContentHandling;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Microsoft.AspNetCore.Hosting;
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
            _builder = new HostBuilder();
        }

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperHttp.Generated");

        public IWebHostBuilder WebHostBuilder => _builder;

        public void Dispose()
        {
            _host?.Dispose();
        }

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

            var url = $"http://localhost:{Hosting.Port}";
            _builder.UseUrls(url);

            if (Hosting.UseKestrel)
            {
                _builder.UseKestrel();
            }

            _builder.UseContentRoot(Hosting.ContentRoot);

            if (Hosting.UseIIS)
            {
                _builder.UseIISIntegration();
            }

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
}
