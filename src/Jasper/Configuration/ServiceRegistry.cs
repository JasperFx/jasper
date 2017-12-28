using System;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static RegistrationExpression<T> For<T>(this IServiceCollection services)
        {
            return new RegistrationExpression<T>(services);
        }

        public static RegistrationExpression<T> ForSingletonOf<T>(this IServiceCollection services)
        {
            return new RegistrationExpression<T>(services, ServiceLifetime.Singleton);
        }


        public class RegistrationExpression<T>
        {
            private readonly ServiceLifetime _lifetime;
            private readonly IServiceCollection _services;

            public RegistrationExpression(IServiceCollection services) : this(services, ServiceLifetime.Transient)
            {
            }

            public RegistrationExpression(IServiceCollection services, ServiceLifetime lifetime)
            {
                _services = services;
                _lifetime = lifetime;
            }

            public void Use(string ignored, Func<IServiceProvider, T> builder)
            {
                Use(builder);
            }

            public void Use(Func<IServiceProvider, T> builder)
            {
                var descriptor = new ServiceDescriptor(typeof(T), s => builder(s), _lifetime);
                _services.Add(descriptor);
            }

            public void Use<TConcrete>() where TConcrete : T
            {
                var descriptor = new ServiceDescriptor(typeof(T), typeof(TConcrete), _lifetime);
                _services.Add(descriptor);
            }

            public void Use(T @object)
            {
                var descriptor = new ServiceDescriptor(typeof(T), @object);
                _services.Add(descriptor);
            }
        }
    }
}
