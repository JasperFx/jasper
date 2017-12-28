using System;
using System.Collections.Generic;
using BlueMilk.Scanning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk
{
    public class ServiceRegistry : List<ServiceDescriptor>, IServiceCollection
    {
        public DescriptorExpression<T> For<T>() where T : class
        {
            return new DescriptorExpression<T>(this, ServiceLifetime.Transient);
        }

        public class DescriptorExpression<T> where T : class
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

            /// <summary>
            /// Fills in a default type implementation for a service type if there are no prior
            /// registrations
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            /// <exception cref="NotImplementedException"></exception>
            public void UseIfNone<TConcrete>() where TConcrete : class, T
            {
                if (_parent.FindDefault<T>() == null)
                {
                    Use<TConcrete>();
                }
            }

            /// <summary>
            /// Delegates to Use<TConcrete>(), polyfill for StructureMap syntax
            /// </summary>
            /// <typeparam name="TConcrete"></typeparam>
            /// <exception cref="NotImplementedException"></exception>
            public DescriptorExpression<T> Add<TConcrete>() where TConcrete : class, T
            {
                return Use<TConcrete>();
            }

            public void Use(T instance)
            {
                _parent.AddSingleton(instance);
            }

            public void Add(T instance)
            {
                _parent.AddSingleton(instance);
            }
        }

        public DescriptorExpression<T> ForSingletonOf<T>() where T : class
        {
            return new DescriptorExpression<T>(this, ServiceLifetime.Singleton);
        }

        public void Scan(Action<IAssemblyScanner> scan)
        {
            var finder = new AssemblyScanner();
            scan(finder);

            finder.Start();

            var descriptor = ServiceDescriptor.Singleton(finder);
            Add(descriptor);
        }


        public void IncludeRegistry<T>() where T : ServiceRegistry, new()
        {
            this.AddRange(new T());
        }
    }
}
