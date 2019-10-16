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
        ///     Overrides a single configuration value. Useful for testing scenarios
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IHostBuilder OverrideConfigValue(this IHostBuilder builder, string key, string value)
        {
            return builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string> {{key, value}});
            });
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application using a custom JasperOptionsBuilder (or JasperRegistry) type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IHostBuilder UseJasper<T>(this IHostBuilder builder, Action<T> overrides = null)
            where T : JasperRegistry, new()
        {
            var registry = new T();
            overrides?.Invoke(registry);
            return builder.UseJasper(registry);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, Action<JasperRegistry> overrides = null)
        {

            var registry = new JasperRegistry();
            overrides?.Invoke(registry);

            return builder.UseJasper(registry);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IHostBuilder UseJasper(this IHostBuilder builder, JasperRegistry registry)
        {
            var appliedKey = "JASPER_HAS_BEEN_APPLIED";
            if (builder.Properties.ContainsKey(appliedKey))
                throw new InvalidOperationException($"{nameof(UseJasper)} can only be called once per builder");

            builder.Properties.Add(appliedKey, "true");

            builder.UseServiceProviderFactory<IServiceCollection>(new LamarServiceProviderFactory());
            builder.UseServiceProviderFactory<ServiceRegistry>(new LamarServiceProviderFactory());

            JasperHost.ApplyExtensions(registry);

            registry.Messaging.StartCompiling(registry);

            registry.Settings.Apply(registry.Services);

            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IHostedService, JasperActivator>();

                s.AddRange(registry.CombineServices());
                s.AddSingleton(registry);
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


        /// <summary>
        ///     Start the application and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost StartJasper(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Start();
            return new JasperRuntime(host);
        }

        /// <summary>
        ///     Builds the application -- but does not start the application -- and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost BuildJasper(this IHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return new JasperRuntime(host);
        }
    }
}
