using System;
using System.Linq;
using Baseline;
using Jasper.Http.Routing;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Http
{
    internal class JasperStartup : IStartup
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

        public static void Register(IContainer container, IServiceCollection services, Router router)
        {
            var serviceDescriptors = services
                .Where(x => x.ServiceType == typeof(IStartup)).ToArray();

            var startups = serviceDescriptors
                .Select(Build)
                .ToArray();

            services.RemoveAll(x => serviceDescriptors.Contains(x));

            services.AddTransient<IStartup>(sp =>
            {
                var others = startups.Select(x => x(sp)).ToArray();
                return new JasperStartup(container, others, router);
            });
        }

        private readonly IContainer _container;
        private readonly IStartup[] _others;
        private readonly Router _router;

        public JasperStartup(IContainer container, IStartup[] others, Router router)
        {
            _container = container;
            _others = others;
            _router = router ?? throw new ArgumentNullException(nameof(router));
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            foreach (var startup in _others)
            {
                startup.ConfigureServices(services);
            }

            if (services.All(x => x.ServiceType != typeof(IServer)))
            {
                services.AddSingleton<IServer, NulloServer>();
            }

            _container.Configure(services);

            bool hasServer = _container.Model.HasRegistrationFor<IServer>();
            if (!hasServer)
            {
                throw new Exception("Got no server!");
            }

            return (IServiceProvider) _container;
        }

        public void Configure(IApplicationBuilder app)
        {
            app.StoreRouter(_router);

            foreach (var startup in _others)
            {
                startup.Configure(app);
            }

            if (!app.HasJasperBeenApplied() && _router.HasAnyRoutes())
            {
                app.Run(_router.Invoke);
            }
        }
    }



}
