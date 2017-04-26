using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperHttp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using StructureMap;

namespace JasperHttp
{
    public class AspNetCoreFeature : IFeature
    {
        private readonly HostBuilder _builder;
        private readonly ServiceRegistry _services;
        private IWebHost _host;

        public AspNetCoreFeature()
        {
            _services = new ServiceRegistry();
            _builder = new HostBuilder(_services);
        }

        public IWebHostBuilder Host => _builder;

        public void Dispose()
        {
            _host?.Dispose();
        }

        public Task<Registry> Bootstrap(JasperRegistry registry)
        {
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
}