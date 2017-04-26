using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Configuration
{
    public class StructureMapServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public StructureMapServiceProviderFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            var registry = new Registry();
            foreach (var service in services)
            {
                register(registry, service);
            }

            Container.Configure(_ => _.AddRegistry(registry));

            return Container;
        }

        private static void register(Registry registry, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.ImplementationType);

                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.CreateFactory());

                return;
            }

            registry.For(descriptor.ServiceType)
                .LifecycleIs(descriptor.Lifetime)
                .Use(descriptor.ImplementationInstance);
        }

        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return new StructureMapServiceProvider(container);
        }
    }
}