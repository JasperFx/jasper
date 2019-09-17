using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Model;
using Jasper.Http.Routing;
using Jasper.Http.Routing.Codegen;
using Jasper.Messaging;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Oakton.AspNetCore;

namespace Jasper
{
    public static class WebHostBuilderExtensions
    {
        /// <summary>
        /// Overrides a single configuration value. Useful for testing scenarios
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
        /// Add Jasper to an ASP.Net Core application using a custom JasperOptionsBuilder (or JasperRegistry) type
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder, Action<T> overrides = null) where T : JasperRegistry, new ()
        {
            var registry = new T();
            overrides?.Invoke(registry);
            return builder.UseJasper(registry);
        }

        /// <summary>
        /// Add Jasper to an ASP.Net Core application with optional configuration to Jasper
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="overrides">Programmatically configure Jasper options</param>
        /// <param name="configure">Programmatically configure Jasper options using the application's IConfiguration and IHostingEnvironment</param>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, Action<JasperRegistry> overrides = null, Action<WebHostBuilderContext, JasperOptions> configure = null)
        {
            var registry = new JasperRegistry(builder.GetSetting(WebHostDefaults.ApplicationKey));
            overrides?.Invoke(registry);

            if (configure != null)
            {
                registry.Settings.Messaging(configure);
            }

            return builder.UseJasper(registry);
        }

        /// <summary>
        /// Add Jasper to an ASP.Net Core application with a pre-built JasperOptionsBuilder
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="registry"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            registry.ConfigureWebHostBuilder(builder);

            // ASP.Net Core will freak out if this isn't there
            builder.UseSetting(WebHostDefaults.ApplicationKey, registry.ApplicationAssembly.FullName);

            JasperHost.ApplyExtensions(registry);

            registry.HttpRoutes.StartFindingRoutes(registry.ApplicationAssembly);
            registry.Messaging.StartCompiling(registry);

            registry.Settings.Apply(registry.Services);

            builder.ConfigureServices(s =>
            {

                if (registry.HttpRoutes.AspNetCoreCompliance == ComplianceMode.GoFaster)
                {
                    s.RemoveAll(x => x.ServiceType == typeof(IStartupFilter) && x.ImplementationType == typeof(AutoRequestServicesStartupFilter));
                }

                s.AddSingleton<IHostedService, JasperActivator>();

                s.AddRange(registry.CombineServices());
                s.AddSingleton(registry);

                s.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
                s.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

                s.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter());
            });

            return builder;
        }

        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";


        /// <summary>
        /// Add Jasper's middleware to the application's RequestDelegate pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static IApplicationBuilder UseJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
                throw new InvalidOperationException("Jasper has already been applied to this web application");

            return Router.BuildOut(app);


        }

        internal static void MarkJasperHasBeenApplied(this IApplicationBuilder builder)
        {
            if (!builder.Properties.ContainsKey(JasperHasBeenApplied))
                builder.Properties.Add(JasperHasBeenApplied, true);
        }

        internal static bool HasJasperBeenApplied(this IApplicationBuilder builder)
        {
            return builder.Properties.ContainsKey(JasperHasBeenApplied);
        }

        /// <summary>
        /// Syntactical sugar to execute the Jasper command line for a configured WebHostBuilder
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public static Task<int> RunJasper(this IWebHostBuilder hostBuilder, string[] args)
        {
            return hostBuilder.RunOaktonCommands(args);
        }


        /// <summary>
        /// Start the application and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost StartJasper(this IWebHostBuilder hostBuilder)
        {
            var host = hostBuilder.Start();
            return new JasperRuntime(host);
        }

        /// <summary>
        /// Builds the application -- but does not start the application -- and return an IJasperHost for the application
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <returns></returns>
        public static IJasperHost BuildJasper(this IWebHostBuilder hostBuilder)
        {
            var host = hostBuilder.Build();
            return new JasperRuntime(host);
        }
    }

    internal class RegisterJasperStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var httpSettings = app.ApplicationServices.GetRequiredService<HttpSettings>();
                if(!httpSettings.Enabled)
                {
                    next(app);
                    return;
                }

                var logger = app.ApplicationServices.GetRequiredService<ILogger<HttpSettings>>();

                app.Use(inner =>
                {
                    return c =>
                    {
                        try
                        {
                            return inner(c);
                        }
                        catch (Exception e)
                        {
                            logger.LogError(e, $"Failed during an HTTP request for {c.Request.Method}: {c.Request.Path}");
                            c.Response.StatusCode = 500;
                            return c.Response.WriteAsync(e.ToString());
                        }
                    };
                });
                next(app);
                if (!app.HasJasperBeenApplied())
                {
                    Router.BuildOut(app).Run(c =>
                    {
                        c.Response.StatusCode = 404;
                        c.Response.Headers["status-description"] = "Resource Not Found";
                        return c.Response.WriteAsync("Resource Not Found");
                    });
                }
            };

        }
    }

    internal class NulloStartup : IStartup
    {
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            return new Container(services);
        }

        public void Configure(IApplicationBuilder app)
        {
            Console.WriteLine("Jasper 'Nullo' startup is being used to start the ASP.Net Core application");
        }
    }
}
