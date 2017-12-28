using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Internals.Codegen;
using Jasper.Internals.Codegen.ServiceLocation;
using Jasper.Internals.Util;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Internals.IoC
{
    public class ServiceGraph
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

        public bool CouldBuild(Type type)
        {
            if (!type.IsConcrete()) return false;

            var ctor = ChooseConstructor(type);
            return ctor != null && CouldBuild(ctor);
        }

        public ServiceDescriptor FindDefault(Type serviceType)
        {
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

        public bool CanResolve(ServiceDescriptor descriptor)
        {
            // TODO -- will deal w/ stuff pulled from IServiceProvider as a lambda
            // much later
            if (descriptor.ImplementationType == null) return false;

            return CouldBuild(descriptor.ImplementationType);
        }

        private ServiceDescriptor TryToDiscover(Type serviceType)
        {
            if (EnumerableStep.IsEnumerable(serviceType)) return null;

            if (serviceType.IsGenericType)
            {
                var basicType = serviceType.GetGenericTypeDefinition();




                var templatedParameterTypes = serviceType.GetGenericArguments();
                var candidates = _services.Where(x => x.ServiceType == basicType && x.ImplementationType != null).Select(x =>
                {
                    return new ServiceDescriptor(serviceType, x.ImplementationType.MakeGenericType(templatedParameterTypes), x.Lifetime);
                }).ToArray();

                _services.AddRange(candidates);

                if (candidates.Any()) return candidates.LastOrDefault();
            }

            if (serviceType.IsConcrete() && !serviceType.IsGenericTypeDefinition && ChooseConstructor(serviceType) != null)
            {
                return new ServiceDescriptor(serviceType, serviceType, ServiceLifetime.Transient);
            }

            return null;
        }

        public ServiceDescriptor[] FindAll(Type serviceType)
        {
            return _services.Where(x => x.ServiceType == serviceType).ToArray();
        }


    }
}
