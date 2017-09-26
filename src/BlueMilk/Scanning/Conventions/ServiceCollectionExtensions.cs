using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace BlueMilk.Scanning.Conventions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddType(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            var hasAlready = services.Any(x => x.ServiceType == serviceType && x.ImplementationType == implementationType);
            if (!hasAlready)
            {
                var serviceDescriptor = new ServiceDescriptor(serviceType, implementationType, ServiceLifetime.Transient);
                services.Add(serviceDescriptor);
            }
        }

        public static ServiceDescriptor FindDefault<T>(this IServiceCollection services)
        {
            return services.FindDefault(typeof(T));
        }

        public static ServiceDescriptor FindDefault(this IServiceCollection services, Type serviceType)
        {
            return services.LastOrDefault(x => x.ServiceType == serviceType);
        }

        public static Type[] RegisteredTypesFor<T>(this IServiceCollection services)
        {
            return services
                .Where(x => x.ServiceType == typeof(T) && x.ImplementationType != null)
                .Select(x => x.ImplementationType)
                .ToArray();
        }
    }
}
