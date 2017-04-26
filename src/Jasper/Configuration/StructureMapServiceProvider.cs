using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Configuration
{
    public sealed class StructureMapServiceProvider : IServiceProvider, ISupportRequiredService
    {
        public StructureMapServiceProvider(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public object GetService(Type serviceType)
        {
            if (serviceType.IsGenericEnumerable())
            {
                // Ideally we'd like to call TryGetInstance here as well,
                // but StructureMap does't like it for some weird reason.
                return GetRequiredService(serviceType);
            }

            return Container.TryGetInstance(serviceType);
        }

        public object GetRequiredService(Type serviceType)
        {
            return Container.GetInstance(serviceType);
        }
    }
}