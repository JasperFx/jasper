using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using Lamar;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper
{
    public partial class JasperRegistry : IWebHostBuilder
    {
        internal bool BootstrappedWithinAspNetCore { get; set; }


        private readonly IHostingEnvironment _hostingEnvironment = new HostingEnvironment();
        private readonly List<Action<WebHostBuilderContext, IServiceCollection>> _configureServicesDelegates
            = new List<Action<WebHostBuilderContext, IServiceCollection>>();

        internal IConfiguration EnvironmentConfiguration { get; } = new ConfigurationBuilder()
            .AddEnvironmentVariables(prefix: "ASPNETCORE_")
            .Build();

        private List<Action<WebHostBuilderContext, IConfigurationBuilder>> _configureAppConfigurationBuilderDelegates
            = new List<Action<WebHostBuilderContext, IConfigurationBuilder>>();


        public IWebHost Build()
        {
            throw new NotSupportedException("Please bootstrap Jasper through JasperRuntime");

        }

        /// <summary>
        /// Adds a delegate for configuring the <see cref="IConfigurationBuilder"/> that will construct an <see cref="IConfiguration"/>.
        /// </summary>
        /// <param name="configureDelegate">The delegate for configuring the <see cref="IConfigurationBuilder" /> that will be used to construct an <see cref="IConfiguration" />.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        /// <remarks>
        /// The <see cref="IConfiguration"/> and <see cref="ILoggerFactory"/> on the <see cref="WebHostBuilderContext"/> are uninitialized at this stage.
        /// The <see cref="IConfigurationBuilder"/> is pre-populated with the settings of the <see cref="IWebHostBuilder"/>.
        /// </remarks>
        public IWebHostBuilder ConfigureAppConfiguration(Action<WebHostBuilderContext, IConfigurationBuilder> configureDelegate)
        {
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            _configureAppConfigurationBuilderDelegates.Add(configureDelegate);
            return this;
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or web application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public IWebHostBuilder ConfigureServices(Action<IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            return ConfigureServices((_ , services) => configureServices(services));
        }

        /// <summary>
        /// Adds a delegate for configuring additional services for the host or web application. This may be called
        /// multiple times.
        /// </summary>
        /// <param name="configureServices">A delegate for configuring the <see cref="IServiceCollection"/>.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public IWebHostBuilder ConfigureServices(Action<WebHostBuilderContext, IServiceCollection> configureServices)
        {
            if (configureServices == null)
            {
                throw new ArgumentNullException(nameof(configureServices));
            }

            _configureServicesDelegates.Add(configureServices);
            return this;
        }

        /// <summary>
        /// Get the setting value from the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to look up.</param>
        /// <returns>The value the setting currently contains.</returns>
        public string GetSetting(string key)
        {
            return EnvironmentConfiguration[key];
        }

        /// <summary>
        /// Add or replace a setting in the configuration.
        /// </summary>
        /// <param name="key">The key of the setting to add or replace.</param>
        /// <param name="value">The value of the setting to add or replace.</param>
        /// <returns>The <see cref="IWebHostBuilder"/>.</returns>
        public IWebHostBuilder UseSetting(string key, string value)
        {
            EnvironmentConfiguration[key] = value;
            return this;
        }



        internal void CopyRegistrations(IWebHostBuilder builder)
        {
            // Copy internal configuration captured in the JasperRegistry
            builder.ConfigureAppConfiguration((context, b) =>
            {
                b.Sources.AddRange(Configuration.Sources);
            });

            foreach (var configureAppConfiguration in _configureAppConfigurationBuilderDelegates)
            {
                builder.ConfigureAppConfiguration(configureAppConfiguration);
            }

            foreach (var configureServices in _configureServicesDelegates)
            {
                builder.ConfigureServices(configureServices);
            }
        }

        internal ServiceRegistry CombineServices(IServiceCollection fromWebHostBuilder)
        {
            // Do Settings here
            var environment = fromWebHostBuilder
                .Where(x => x.ServiceType == typeof(IHostingEnvironment))
                .Select(x => x.ImplementationInstance).OfType<IHostingEnvironment>()
                .FirstOrDefault();

            var configuration = fromWebHostBuilder
                .Where(x => x.ServiceType == typeof(IConfiguration))
                .Select(x => x.ImplementationInstance).OfType<IConfiguration>()
                .FirstOrDefault();

            Settings.Bootstrap(new WebHostBuilderContext
            {
                Configuration = configuration,
                HostingEnvironment = environment
            });

            var all = fromWebHostBuilder.Concat(_baseServices).Concat(ExtensionServices).Concat(_applicationServices);


            var combined = new ServiceRegistry();
            combined.AddRange(all);

            combined.Insert(0, ServiceDescriptor.Singleton<IHostedService, MessagingActivator>());

            combined.For<IDurableMessagingFactory>().UseIfNone<NulloDurableMessagingFactory>();

            combined.For<IServer>().UseIfNone<NulloServer>();

            return combined;
        }

        internal ServiceRegistry CompileConfigurationAndServicesForIdiomaticBootstrapping()
        {
            _baseServices.Insert(0, ServiceDescriptor.Singleton<IHostedService, MessagingActivator>());

            // Add services
            var options = new WebHostOptions(EnvironmentConfiguration);
            var contentRootPath = ResolveContentRootPath(options.ContentRootPath, AppContext.BaseDirectory);

            // Initialize the hosting environment
            _hostingEnvironment.Initialize(ServiceName, contentRootPath, options);

            var context = new WebHostBuilderContext
            {
                Configuration = EnvironmentConfiguration,
                HostingEnvironment = _hostingEnvironment
            };

            Services.AddSingleton(_hostingEnvironment);
            Services.AddSingleton(context);

            var builder = Configuration
                .SetBasePath(_hostingEnvironment.ContentRootPath)
                .AddInMemoryCollection(EnvironmentConfiguration.AsEnumerable());

            foreach (var configureAppConfiguration in _configureAppConfigurationBuilderDelegates)
            {
                configureAppConfiguration(context, builder);
            }

            var configuration = builder.Build();
            Services.AddSingleton<IConfiguration>(configuration);
            Services.AddSingleton<IConfigurationRoot>(configuration);

            context.Configuration = configuration;


            Settings.Bootstrap(new WebHostBuilderContext
            {
                HostingEnvironment = _hostingEnvironment,
                Configuration = configuration
            });

            var listener = new DiagnosticListener("Microsoft.AspNetCore");
            Services.AddSingleton(listener);
            Services.AddSingleton<DiagnosticSource>(listener);

            Services.AddTransient<IApplicationBuilderFactory, ApplicationBuilderFactory>();
            Services.AddTransient<IHttpContextFactory, HttpContextFactory>();
            Services.AddScoped<IMiddlewareFactory, MiddlewareFactory>();
            Services.AddOptions();
            Services.AddLogging();

            Services.AddSingleton<IApplicationLifetime, ApplicationLifetime>();

            // Conjure up a RequestServices
            // TODO -- want this to be optional
            Services.AddTransient<IStartupFilter, AutoRequestServicesStartupFilter>();

            foreach (var configureServices in _configureServicesDelegates)
            {
                configureServices(context, Services);
            }

            var all = _baseServices.Concat(ExtensionServices).Concat(_applicationServices);

            var combined = new ServiceRegistry();
            combined.AddRange(all);

            combined.For<IDurableMessagingFactory>().UseIfNone<NulloDurableMessagingFactory>();
            combined.For<IServer>().UseIfNone<NulloServer>();

            return combined;
        }

        private string ResolveContentRootPath(string contentRootPath, string basePath)
        {
            if (string.IsNullOrEmpty(contentRootPath))
            {
                return basePath;
            }
            if (Path.IsPathRooted(contentRootPath))
            {
                return contentRootPath;
            }
            return Path.Combine(Path.GetFullPath(basePath), contentRootPath);
        }

    }
}
