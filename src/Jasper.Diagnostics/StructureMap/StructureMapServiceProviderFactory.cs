using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Diagnostics.StructureMap
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
