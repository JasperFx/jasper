using System;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace JasperHttp.Configuration
{
    internal class HostBuilder : IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;
        private readonly ServiceRegistry _services;


        public HostBuilder(ServiceRegistry services)
        {

            _services = services;
            _inner = new WebHostBuilder();
            _inner.ConfigureServices(_ =>
            {
                _.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
            });

           
        }

        public IWebHost Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        public IWebHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            return _inner.UseLoggerFactory(loggerFactory);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            return _inner.ConfigureLogging(configureLogging);
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        public string GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }

        internal IWebHost Activate(IContainer container)
        {
            _inner.ConfigureServices(services => JasperStartup.Register(container, services));
            return _inner.Build();
        }
    }

    public class JasperStartup : IStartup
    {
        public static IStartup Build(IServiceProvider provider, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance.As<IStartup>();
            }

            if (descriptor.ImplementationType != null)
            {
                return provider.GetService(descriptor.ServiceType).As<IStartup>();
            }

            return descriptor.ImplementationFactory(provider).As<IStartup>();
        }

        public static Func<IServiceProvider, IStartup> Build(ServiceDescriptor service)
        {
            return sp => Build(sp, service);
        }

        public static void Register(IContainer container, IServiceCollection services)
        {
            var startups = services
                .Where(x => x.ServiceType == typeof(IStartup))
                .Select(Build)
                .ToArray();

            services.AddTransient<IStartup>(sp =>
            {
                var others = startups.Select(x => x(sp)).ToArray();
                return new JasperStartup(container, others);
            });
        }

        private readonly IContainer _container;
        private readonly IStartup[] _others;

        public JasperStartup(IContainer container, IStartup[] others)
        {
            // TODO -- WHAT IF THERE ARE NO STARTUPS HERE?
            _container = container;
            _others = others;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            foreach (var startup in _others)
            {
                startup.ConfigureServices(services);
            }

            var registry = new Registry();
            foreach (var service in services)
            {
                registry.Register(service);
            }

            _container.Configure(x => x.AddRegistry(registry));

            return new StructureMapServiceProvider(_container);
        }

        public void Configure(IApplicationBuilder app)
        {
            foreach (var startup in _others)
            {
                startup.Configure(app);
            }
        }
    }
}