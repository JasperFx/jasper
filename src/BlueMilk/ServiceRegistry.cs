using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlueMilk
{
    public class ServiceRegistry : ServiceCollection
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
    }
}
