using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Oakton.AspNetCore;

namespace Jasper
{
    public static class WebHostBuilderExtensions
    {


        /// <summary>
        ///     Overrides a single configuration value. Useful for testing scenarios
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static IWebHostBuilder OverrideConfigValue(this IWebHostBuilder builder, string key, string value)
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
        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder, Action<T> overrides = null)
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
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, Action<JasperRegistry> overrides = null)
        {
            var registry = new JasperRegistry(builder.GetSetting(WebHostDefaults.ApplicationKey));
            overrides?.Invoke(registry);

            return builder.UseJasper(registry);
        }

        /// <summary>
        ///     Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            var appliedKey = "JASPER_HAS_BEEN_APPLIED";
            if (builder.GetSetting(appliedKey).IsNotEmpty())
            {
                throw new InvalidOperationException($"{nameof(UseJasper)} can only be called once per builder");
            }

            builder.UseSetting(appliedKey, "true");

            registry.ConfigureWebHostBuilder(builder);

            // ASP.Net Core will freak out if this isn't there
            builder.UseSetting(WebHostDefaults.ApplicationKey, registry.ApplicationAssembly.FullName);

            JasperHost.ApplyExtensions(registry);

            registry.Messaging.StartCompiling(registry);

            registry.Settings.Apply(registry.Services);

            builder.ConfigureServices(s =>
            {

                s.AddSingleton<IHostedService, JasperActivator>();

                s.AddRange(registry.CombineServices());
                s.AddSingleton(registry);

                s.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
                s.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();
                
                // Registers an empty startup if there is none in the application
                if (s.All(x => x.ServiceType != typeof(IStartup))) s.AddSingleton<IStartup>(new NulloStartup());

                // Registers a "nullo" server if there is none in the application
                // i.e., Kestrel isn't applied
                if (s.All(x => x.ServiceType != typeof(IServer))) s.AddSingleton<IServer>(new NulloServer());

            });

            return builder;
        }





        /// <summary>
        ///     Syntactical sugar to execute the Jasper command line for a configured WebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> RunJasper(this IWebHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.RunOaktonCommands(args);
        }


        /// <summary>
        ///     Start the application and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost StartJasper(this IWebHostBuilder hostBuilder)
        {
            var host = hostBuilder.Start();
            return new JasperRuntime(host);
        }

        /// <summary>
        ///     Builds the application -- but does not start the application -- and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost BuildJasper(this IWebHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return new JasperRuntime(host);
        }
    }
}
