using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlueMilk.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk
{
    public class ServiceRegistry : List<ServiceDescriptor>, IServiceCollection
    {
        public DescriptorExpression<T> For<T>()
        {
            return new DescriptorExpression<T>(this, ServiceLifetime.Transient);
        }

        public class DescriptorExpression<T>
        {
            private readonly ServiceRegistry _parent;
            private readonly ServiceLifetime _lifetime;
            private ServiceDescriptor _descripter;

            public DescriptorExpression(ServiceRegistry parent, ServiceLifetime lifetime)
            {
                _parent = parent;
                _lifetime = lifetime;
            }

            public DescriptorExpression<T> Use<TConcrete>() where TConcrete : class, T
            {
                _descripter = new ServiceDescriptor(typeof(T), typeof(TConcrete), _lifetime);

                _parent.Add(_descripter);

                return this;
            }
        }

        public DescriptorExpression<T> ForSingletonOf<T>()
        {
            return new DescriptorExpression<T>(this, ServiceLifetime.Singleton);
        }

        public void Scan(Action<IAssemblyScanner> scan)
        {
            // TODO -- might push in the application assembly here first
            var finder = new AssemblyScanner();
            scan(finder);

            finder.Start();

            var descriptor = ServiceDescriptor.Singleton(finder);
            Add(descriptor);
        }

        public async Task ApplyScannedTypes()
        {
            foreach (var scanner in this.Select(x => x.ImplementationInstance).OfType<AssemblyScanner>().ToArray())
            {
                await scanner.ApplyRegistrations(this);
            }
        }
    }
}
