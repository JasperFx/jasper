using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Configuration
{
    public sealed class StructureMapServiceProvider : IServiceProvider, ISupportRequiredService, IDisposable
    {
        private JasperRuntime _runtime;

        public StructureMapServiceProvider(IContainer container)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));

            Container = container;
            if (Container.Role == ContainerRole.Root)
            {
                _runtime = container.GetInstance<JasperRuntime>();
            }
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

        public void Dispose()
        {
            _runtime?.Dispose();
        }
    }
}
