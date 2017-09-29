using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BlueMilk.Codegen;
using BlueMilk.Codegen.ServiceLocation;
using BlueMilk.Util;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.IoC
{
    public class ServiceGraph : IVariableSource
    {
        private readonly IServiceCollection _services;
        private readonly Dictionary<Type, ConstructorInfo> _constructors = new Dictionary<Type, ConstructorInfo>();

        public ServiceGraph(IServiceCollection services)
        {
            _services = services;
        }

        public ConstructorInfo ChooseConstructor(Type type)
        {
            if (_constructors.ContainsKey(type)) return _constructors[type];

            var constructor = type.GetTypeInfo()
                .GetConstructors()
                .OrderByDescending(x => x.GetParameters().Length)
                .FirstOrDefault(CouldBuild);

            _constructors[type] = constructor;

            return constructor;
        }

        public bool CouldBuild(ConstructorInfo ctor)
        {
            return ctor.GetParameters().Length == 1 || ctor.GetParameters().All(x => FindDefault(x.ParameterType) != null);
        }

        public ServiceDescriptor FindDefault(Type serviceType)
        {
            // TODO -- fill in by closing a generic -- LATER!!!!

            var candidate = _services.LastOrDefault(x => x.ServiceType == serviceType);

            if (candidate == null)
            {
                candidate = TryToDiscover(serviceType);
                if (candidate != null)
                {
                    _services.Add(candidate);
                }
            }

            return candidate;
        }

        private ServiceDescriptor TryToDiscover(Type serviceType)
        {
            if (serviceType.IsConcrete() && ChooseConstructor(serviceType) != null)
            {
                return new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Transient);
            }

            return null;
        }

        public ServiceDescriptor[] FindAll(Type serviceType)
        {
            return _services.Where(x => x.ServiceType == serviceType).ToArray();
        }

        public bool Matches(Type type)
        {
            return type == typeof(IServiceScopeFactory) || type == typeof(IServiceProvider) || _services.Any(x => x.ServiceType == type);
        }

        public Variable Create(Type type)
        {
            if (type == typeof(IServiceScopeFactory))
            {
                return new InjectedField(typeof(IServiceScopeFactory));
            }

            if (type == typeof(IServiceProvider))
            {
                return new ServiceScopeFactoryCreation().Provider;
            }

            return new ServiceCreationFrame(type).Service;
        }
    }
}
