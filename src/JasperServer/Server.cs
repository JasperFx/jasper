using System;
using System.IO;
using System.Threading.Tasks;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperBus;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace JasperServer
{
    public class HybridJasperRegistry : JasperBusRegistry
    {
        private readonly HybridServerFeature _feature;

        public HybridJasperRegistry()
        {
            _feature = Feature<HybridServerFeature>();
        }

        public HostingConfiguration Host => _feature.Host;
    }

    public class HostingConfiguration
    {
        public bool UseKestrel { get; set; } = true;
        public bool UseIIS { get; set; } = true;
        public int Port { get; set; } = 3000;
        public string ContentRoot { get; set; } = Directory.GetCurrentDirectory();
    }

    public class HybridServerFeature : IFeature
    {
        private HybridJasperServer _server;

        public readonly Registry Services = new ServiceRegistry();
        public readonly HostingConfiguration Host = new HostingConfiguration();

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return Task.FromResult(Services);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(()=>
            {
                _server = new HybridJasperServer(Host);
                _server.Start(runtime.Container);
            });
        }

        void IDisposable.Dispose()
        {
            _server?.Dispose();
        }
    }

    public class HybridJasperServer : IDisposable
    {
        private readonly HostingConfiguration _config;
        private IWebHost _host;

        public HybridJasperServer(HostingConfiguration config)
        {
            _config = config;
        }

        public void Start(IContainer container)
        {
            var url = $"http://localhost:{_config.Port}";
            Console.WriteLine($"Server Listening on {_config.Port}");
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(_config.ContentRoot)
                .UseUrls(url)
                .UseStructureMap(container)
                .UseStartup<HybridServerStartup>()
                .Build();

            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }

    internal class HybridServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void ConfigureContainer(IContainer container)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Run(async http =>
            {
                http.Response.StatusCode = 200;
                http.Response.ContentType = "text/plain";
                await http.Response.WriteAsync($"Nothing to see here at {DateTime.Now}.");
            });
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IWebHostBuilder UseStructureMap(this IWebHostBuilder builder, IContainer container)
        {
            return builder.ConfigureServices(services => services.AddStructureMap(container));
        }

        public static IServiceCollection AddStructureMap(this IServiceCollection services, IContainer container)
        {
            return services.AddSingleton<IServiceProviderFactory<IContainer>>(new StructureMapServiceProviderFactory(container));
        }
    }

    public class StructureMapServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public StructureMapServiceProviderFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            var registry = Container ?? new Container();

            registry.Populate(services);

            return registry;
        }

        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return new StructureMapServiceProvider(container);
        }
    }
}
