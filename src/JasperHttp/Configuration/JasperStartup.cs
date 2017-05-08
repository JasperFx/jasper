using System;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace JasperHttp.Configuration
{
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
                .ToArray<Func<IServiceProvider, IStartup>>();

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