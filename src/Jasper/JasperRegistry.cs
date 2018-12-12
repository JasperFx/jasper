using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Baseline;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Http.Routing;
using Jasper.Messaging.Transports;
using Jasper.Settings;
using Jasper.Util;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper
{
    /// <summary>
    ///     Completely defines and configures a Jasper application
    /// </summary>
    public class JasperRegistry : JasperOptionsBuilder, IWebHostBuilder
    {
        public JasperRegistry()
        {
            // Specific to JasperRegistry
            Hosting = this;
            deriveServiceName();

            // ASP.Net Core will freak out if this isn't there
            Hosting.UseSetting(WebHostDefaults.ApplicationKey, ApplicationAssembly.FullName);
        }


        /// <summary>
        ///     Configure ASP.Net Core hosting for this Jasper application
        /// </summary>
        public IWebHostBuilder Hosting { get; }

        private void deriveServiceName()
        {
            if (GetType() == typeof(JasperRegistry))
                ServiceName = ApplicationAssembly?.GetName().Name ?? "JasperService";
            else
                ServiceName = GetType().Name.Replace("JasperRegistry", "").Replace("Registry", "");
        }


        internal IWebHostBuilder InnerWebHostBuilder { get; } = createDefaultWebHostBuilder();

        private static IWebHostBuilder createDefaultWebHostBuilder()
        {
            var builder = new WebHostBuilder();

            builder.ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = hostingContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                    config.AddEnvironmentVariables();

                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();

                });

            return builder;
        }

        IWebHost IWebHostBuilder.Build()
        {
            throw new NotSupportedException("Please bootstrap Jasper through JasperRuntime");
        }

        /// <summary>
        ///     Adds a delegate for configuring the <see cref="IConfigurationBuilder" /> that will construct an
        ///     <see cref="IConfiguration" />.
        /// </summary>
        /// <param name="configureDelegate">
        ///     The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used
        ///     to construct an <see cref="IConfiguration" />.
        /// </param>
        /// <returns>The <see cref="IWebHostBuilder" />.</returns>
        /// <remarks>
        ///     The <see cref="IConfiguration" /> and <see cref="ILoggerFactory" /> on the <see cref="WebHostBuilderContext" /> are
        ///     uninitialized at this stage.
        ///     The <see cref="IConfigurationBuilder" /> is pre-populated with the settings of the <see cref="IWebHostBuilder" />.
        /// </remarks>
        IWebHostBuilder IWebHostBuilder.ConfigureAppConfiguration(
            Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            InnerWebHostBuilder.ConfigureAppConfiguration(configureDelegate);

            return this;
        }

        /// <summary>
        ///     Adds a delegate for configuring additional services for the host or web application. This may be called
        ///     multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection" />.</param>
        /// <returns>The <see cref="IWebHostBuilder" />.</returns>
        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<IServiceCollection> configureServices)
        {
            InnerWebHostBuilder.ConfigureServices(configureServices);
            return this;
        }

        /// <summary>
        ///     Adds a delegate for configuring additional services for the host or web application. This may be called
        ///     multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection" />.</param>
        /// <returns>The <see cref="IWebHostBuilder" />.</returns>
        IWebHostBuilder IWebHostBuilder.ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            InnerWebHostBuilder.ConfigureServices(configureServices);
            return this;
        }

        /// <summary>
        ///     Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        string IWebHostBuilder.GetSetting(string key)
        {
            return InnerWebHostBuilder.GetSetting(key);
        }

        /// <summary>
        ///     Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IWebHostBuilder" />.</returns>
        IWebHostBuilder IWebHostBuilder.UseSetting(string key, string value)
        {
            InnerWebHostBuilder.UseSetting(key, value);
            return this;
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

        /// <summary>
        /// Used by Jasper's Alba adapter. Creates the equivalent WebHostBuilder
        /// for the configurations here.
        /// </summary>
        /// <returns></returns>
        public IWebHostBuilder ToWebHostBuilder()
        {
            return InnerWebHostBuilder.ConfigureServices(s =>
            {
                if (s.All(x => x.ServiceType != typeof(IStartup)))
                {
                    s.AddSingleton<IStartup>(new JasperRegistry.NulloStartup());
                }

                if (s.All(x => x.ServiceType != typeof(IServer)))
                {
                    s.AddSingleton<IServer>(new NulloServer());
                }

                s.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter());
            })
                .UseJasper(this);

        }
    }
}
