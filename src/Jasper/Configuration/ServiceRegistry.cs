using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using StructureMap;
using StructureMap.Pipeline;

namespace Jasper.Configuration
{
    public class ServiceRegistry : Registry, IServiceCollection
    {
        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public SmartInstance<TImplementation, TService> SetServiceIfNone<TService, TImplementation>()
            where TImplementation : TService
        {
            return For<TService>().UseIfNone<TImplementation>();
        }


        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        public void SetServiceIfNone(Type type, Instance instance)
        {
            For(type).Configure(x => x.Fallback = instance);
        }


        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public ObjectInstance SetServiceIfNone<TService>(TService value)
        {
            return For<TService>().UseIfNone(value);
        }

        /// <summary>
        /// Sets the instanceault implementation of a service if there is no
        /// previous registration
        /// </summary>
        /// <param name="interfaceType"></param>
        /// <param name="concreteType"></param>
        /// <returns></returns>
        public Instance SetServiceIfNone(Type interfaceType, Type concreteType)
        {
            var instance = new ConfiguredInstance(concreteType);
            For(interfaceType).Configure(x => x.Fallback = instance);
            return instance;
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        /// <returns></returns>
        public Instance AddService<TService, TImplementation>() where TImplementation : TService
        {
            return For<TService>().Add<TImplementation>();
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="implementation"></param>
        /// <returns></returns>
        public Instance AddService<TService>(Type implementationType)
        {
            var instance = new ConfiguredInstance(implementationType);

            For<TService>().AddInstance(instance);

            return instance;
        }

        /// <summary>
        /// Registers a instanceault implementation for a service.  Overwrites any existing
        /// registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <typeparam name="TImplementation"></typeparam>
        public SmartInstance<TImplementation, TService> ReplaceService<TService, TImplementation>()
            where TImplementation : TService
        {
            return For<TService>().ClearAll().Use<TImplementation>();
        }

        /// <summary>
        /// Registers a instanceault implementation for a service.  Overwrites any existing
        /// registration
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public ObjectInstance<TService, TService> ReplaceService<TService>(TService value) where TService : class
        {
            return For<TService>().ClearAll().Use(value);
        }

        /// <summary>
        /// Registers an *additional* implementation of a service.  Actual behavior varies by actual
        /// IoC container
        /// </summary>
        /// <typeparam name="TService"></typeparam>
        /// <param name="value"></param>
        public void AddService<TService>(TService value) where TService : class
        {
            For<TService>().Add(value);
        }

        /// <summary>
        /// Removes any registrations for type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddService(Type type, Instance instance)
        {
            For(type).Add(instance);
        }

        public void ReplaceService(Type type, Instance @default)
        {
            For(type).ClearAll();
            For(type).Use(@default);
        }

        private readonly List<ServiceDescriptor> _descriptors = new List<ServiceDescriptor>();

        /// <inheritdoc />
        public int Count => _descriptors.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        public ServiceDescriptor this[int index]
        {
            get
            {
                return _descriptors[index];
            }
            set
            {
                _descriptors[index] = value;
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            _descriptors.Clear();
        }

        /// <inheritdoc />
        public bool Contains(ServiceDescriptor item)
        {
            return _descriptors.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
        {
            _descriptors.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(ServiceDescriptor item)
        {
            return _descriptors.Remove(item);
        }

        /// <inheritdoc />
        public IEnumerator<ServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        void ICollection<ServiceDescriptor>.Add(ServiceDescriptor item)
        {
            _descriptors.Add(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(ServiceDescriptor item)
        {
            return _descriptors.IndexOf(item);
        }

        public void Insert(int index, ServiceDescriptor item)
        {
            _descriptors.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            _descriptors.RemoveAt(index);
        }

        internal void BakeAspNetCoreServices()
        {
            this.Populate(_descriptors);
        }
    }
}
