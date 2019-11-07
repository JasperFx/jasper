using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging;
using Lamar;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace Jasper
{
    public static class HostBuilderExtensions
    {

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application using a custom JasperOptionsBuilder (or JasperRegistry) type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IHostBuilder UseJasper<T>(this IHostBuilder builder, Action<HostBuilderContext, JasperRegistry> overrides = null)
            where T : JasperRegistry, new()
        {
            return builder.UseJasper(new T(), overrides);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, Action<HostBuilderContext, JasperRegistry> overrides = null)
        {

            var registry = new JasperRegistry();

            return builder.UseJasper(registry, overrides);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, Action<JasperRegistry> overrides)
        {
            return builder.UseJasper(new JasperRegistry(), (c, r) => overrides(r));
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, JasperRegistry registry, Action<HostBuilderContext, JasperRegistry> customization = null)
        {
            var appliedKey = "JASPER_HAS_BEEN_APPLIED";
            if (builder.Properties.ContainsKey(appliedKey))
                throw new InvalidOperationException($"{nameof(UseJasper)} can only be called once per builder");

            builder.Properties.Add(appliedKey, "true");

            builder.UseServiceProviderFactory<IServiceCollection>(new LamarServiceProviderFactory());
            builder.UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory());

            ExtensionLoader.ApplyExtensions(registry);

            builder.ConfigureServices((context, services) =>
            {
                customization?.Invoke(context, registry);

                registry.Messaging.StartCompiling(registry);

                registry.Settings.Apply(registry.Services);

                services.AddSingleton(registry.Options);
                services.AddSingleton<IHostedService, JasperActivator>();

                services.AddRange(registry.CombineServices());
                services.AddSingleton(registry);
            });

            return builder;
        }


        /// <summary>
        ///     Syntactical sugar to execute the Jasper command line for a configured WebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> RunJasper(this IHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.RunOaktonCommands(args);
        }

        public static T Get<T>(this IHost host)
        {
            return host.Services.GetRequiredService<T>();
        }

        public static object Get(this IHost host, Type serviceType)
        {
            return host.Services.GetRequiredService(serviceType);
        }

        /// <summary>
        /// Syntactical sugar for host.Services.GetRequiredService<IMessagePublisher>().Send(message)
        /// </summary>
        /// <param name="host"></param>
        /// <param name="message"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Task Send<T>(this IHost host, T message)
        {
            return host.Get<IMessagePublisher>().Send(message);
        }

        public static Task Invoke<T>(this IHost host, T command)
        {
            return host.Get<ICommandBus>().Invoke(command);
        }
    }
}
