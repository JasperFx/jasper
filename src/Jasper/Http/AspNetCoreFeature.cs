using System.IO;
using System.Threading.Tasks;
using Baseline;
using Jasper.Codegen;
using Jasper.Configuration;
using Jasper.Http.Configuration;
using Microsoft.AspNetCore.Hosting;
using StructureMap;

namespace Jasper.Http
{
    public class AspNetCoreFeature : IFeature
    {
        private readonly HostBuilder _builder;
        private readonly ServiceRegistry _services;
        private IWebHost _host;

        public AspNetCoreFeature()
        {
            _services = new ServiceRegistry();
            _builder = new HostBuilder();
        }

        public IWebHostBuilder WebHostBuilder => _builder;

        public void Dispose()
        {
            _host?.Dispose();
        }

        public HostingConfiguration Hosting { get; } = new HostingConfiguration();

        public Task<Registry> Bootstrap(JasperRegistry registry)
        {
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

            return Task.FromResult(_services.As<Registry>());
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
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
