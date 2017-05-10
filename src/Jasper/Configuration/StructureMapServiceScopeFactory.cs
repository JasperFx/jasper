using System;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;

namespace Jasper.Configuration
{
    internal sealed class StructureMapServiceScopeFactory : IServiceScopeFactory
    {
        public StructureMapServiceScopeFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IServiceScope CreateScope()
        {
            return new StructureMapServiceScope(Container.GetNestedContainer());
        }

        private class StructureMapServiceScope : IServiceScope
        {
            public StructureMapServiceScope(IContainer container)
            {
                Container = container;
                ServiceProvider = container.GetInstance<IServiceProvider>();
            }

            private IContainer Container { get; }

            public IServiceProvider ServiceProvider { get; }

            public void Dispose() => Container.Dispose();
        }
    }
}