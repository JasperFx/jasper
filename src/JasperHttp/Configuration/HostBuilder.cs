using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Jasper.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;
using StructureMap.Configuration.DSL.Expressions;
using StructureMap.Graph;
using StructureMap.Pipeline;

namespace JasperHttp.Configuration
{
    internal class HostBuilder : IWebHostBuilder
    {
        private readonly WebHostBuilder _inner;
        private readonly ServiceRegistry _services;


        public HostBuilder(ServiceRegistry services)
        {
            _services = services;
            _inner = new WebHostBuilder();
            
        }

        public IWebHost Build()
        {
            throw new NotSupportedException("Jasper needs to do the web host building within its bootstrapping");
        }

        public IWebHostBuilder UseLoggerFactory(ILoggerFactory loggerFactory)
        {
            return _inner.UseLoggerFactory(loggerFactory);
        }

        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            return _inner.ConfigureServices(configureServices);
        }

        public IWebHostBuilder ConfigureLogging(Action<ILoggerFactory> configureLogging)
        {
            return _inner.ConfigureLogging(configureLogging);
        }

        public IWebHostBuilder UseSetting(string key, string value)
        {
            return _inner.UseSetting(key, value);
        }

        public string GetSetting(string key)
        {
            return _inner.GetSetting(key);
        }

        internal IWebHost Activate(IContainer container)
        {
            _inner.UseStructureMap(container);
            return _inner.Build();
        }
    }

    public static class ServiceCollectionExtensions
    {
        public static IWebHostBuilder UseStructureMap(this IWebHostBuilder builder, IContainer container)
        {
            return builder.ConfigureServices(services => services.AddStructureMap(container));
        }

        public static IServiceCollection AddStructureMap(this IServiceCollection services, IContainer container)
        {
            return services.AddSingleton<IServiceProviderFactory<IContainer>>(new StructureMapServiceProviderFactory(container));
        }
    }

    public class StructureMapServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public StructureMapServiceProviderFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            var registry = new Registry();
            foreach (var service in services)
            {
                register(registry, service);
            }

            Container.Configure(_ => _.AddRegistry(registry));

            return Container;
        }

        private static void register(Registry registry, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.ImplementationType);

                return;
            }

            if (descriptor.ImplementationFactory != null)
            {
                registry.For(descriptor.ServiceType)
                    .LifecycleIs(descriptor.Lifetime)
                    .Use(descriptor.CreateFactory());

                return;
            }

            registry.For(descriptor.ServiceType)
                .LifecycleIs(descriptor.Lifetime)
                .Use(descriptor.ImplementationInstance);
        }

        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return new StructureMapServiceProvider(container);
        }
    }

    internal static class HelperExtensions
    {
        public static Expression<Func<IContext, object>> CreateFactory(this ServiceDescriptor descriptor)
        {
            return context => descriptor.ImplementationFactory(context.GetInstance<IServiceProvider>());
        }

        public static bool IsGenericEnumerable(this Type type)
        {
            return type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static GenericFamilyExpression LifecycleIs(this GenericFamilyExpression instance, ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return instance.LifecycleIs(Lifecycles.Singleton);
                case ServiceLifetime.Scoped:
                    return instance.LifecycleIs(Lifecycles.Container);
                case ServiceLifetime.Transient:
                    return instance.LifecycleIs(Lifecycles.Unique);
                default:
                    throw new ArgumentOutOfRangeException(nameof(lifetime), lifetime, null);
            }
        }

        public static bool HasFamily<TPlugin>(this PluginGraph graph)
        {
            return graph.HasFamily(typeof(TPlugin));
        }
    }
}