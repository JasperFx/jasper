using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Baseline;
using Baseline.Reflection;
using Jasper.Configuration;
using Jasper.EnvironmentChecks;
using Jasper.Http;
using Jasper.Messaging;
using Lamar;
using Lamar.Scanning.Conventions;
using Lamar.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Builder;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper
{
    public partial class JasperRuntime
    {
        private IHostedService[] _hostedServices;
        private IServer _server;
        private ApplicationLifetime _applicationLifetime;
        private ILogger _logger;
        private bool _hasServer;

        private async Task startHostedServices()
        {
            _hostedServices = Container.GetAllInstances<IHostedService>().ToArray();

            foreach (var hostedService in _hostedServices)
                await hostedService.StartAsync(Registry.MessagingSettings.Cancellation);
        }

        private static async Task<JasperRuntime> bootstrap(JasperRegistry registry)
        {
            var timer = new PerfTimer();
            timer.Start("Bootstrapping");

            timer.Record("Finding and Applying Extensions", () =>
            {
                applyExtensions(registry);
            });

            var buildingServices = Task.Factory.StartNew(() =>
            {
                return timer.Record("Combining Services and Building Settings", registry.CompileAspNetConfiguration);
            });



            var handlerCompilation = registry.Messaging.CompileHandlers(registry, timer);


            var runtime = new JasperRuntime(registry, timer);

            var services = await buildingServices;
            services.AddSingleton(runtime);


            // TODO -- need to pass in the perf timer here
            var container = await Lamar.Container.BuildAsync(services);
            container.DisposalLock = DisposalLock.Ignore;
            runtime.Container = container;

            var routeDiscovery = registry.HttpRoutes.Enabled
                ? registry.HttpRoutes.FindRoutes(runtime, registry, timer)
                : Task.CompletedTask;

            runtime.buildAspNetCoreServer();

            await routeDiscovery;

            await Task.WhenAll(runtime.startAspNetCoreServer(), handlerCompilation, runtime.startHostedServices());


            // Run environment checks
            timer.Record("Environment Checks", () =>
            {
                var recorder = EnvironmentChecker.ExecuteAll(runtime);
                if (registry.MessagingSettings.ThrowOnValidationErrors) recorder.AssertAllSuccessful();
            });

            timer.Stop();

            return runtime;
        }

        private static void applyExtensions(JasperRegistry registry)
        {
            var assemblies = FindExtensionAssemblies();

            if (!assemblies.Any()) return;

            var extensions = assemblies
                .Select(x => x.GetAttribute<JasperModuleAttribute>().ExtensionType)
                .Where(x => x != null)
                .Select(x => Activator.CreateInstance(x).As<IJasperExtension>())
                .ToArray();

            registry.ApplyExtensions(extensions);
        }

        public static Assembly[] FindExtensionAssemblies()
        {
            return AssemblyFinder
                .FindAssemblies(txt => { }, false)
                .Where(a => a.HasAttribute<JasperModuleAttribute>())
                .ToArray();
        }

        private async Task shutdownAspNetCoreServer()
        {
            // TODO -- get out of here if bootstrapped by ASP.Net Core or
            // aspnet is disabled
            // or no server

            // TODO -- not sure yet where the extension method is for this
            //_logger?.Shutdown();

            // TODO -- may need a timeout on this thing
            await _server.StopAsync(Registry.MessagingSettings.Cancellation).ConfigureAwait(false);

            // Fire IApplicationLifetime.Stopped
            _applicationLifetime?.NotifyStopped();

            HostingEventSource.Log.HostStop();
        }

        public RequestDelegate RequestDelegate { get; private set; }

        private async Task startAspNetCoreServer()
        {
            // TODO -- don't do anything if bootstrapped by ASP.Net Core
            // TODO -- don't do anything if aspnet stuff is disabled


            HostingEventSource.Log.HostStart();

            // TODO -- pick up some tracing here from WebHost
            _logger = Get<ILoggerFactory>().CreateLogger("Jasper");

            // TODO -- probably going to vote to parallelize this
            buildAspNetCoreServer();

            _applicationLifetime = Container.GetInstance<IApplicationLifetime>().As<ApplicationLifetime>();

            var httpContextFactory = Container.QuickBuild<HttpContextFactory>();

            var hostingApp = new HostingApplication(
                RequestDelegate,
                _logger,
                Container.GetInstance<DiagnosticListener>(), // See if this can be passed in directly
                httpContextFactory);

            await _server.StartAsync(hostingApp, Registry.MessagingSettings.Cancellation);

            // Fire IApplicationLifetime.Started
            _applicationLifetime?.NotifyStarted();

            // TODO -- log that the application started
            // _logger.Started(); // dunno where this comes from yet
        }

        private void buildAspNetCoreServer()
        {
            _server = buildServer();
            _hasServer = !(_server is NulloServer);

            RequestDelegate = buildRequestDelegate(_server);

            HttpAddresses = Registry.HttpAddresses?.Split(';').ToArray() ??
                            new string[0];

        }

        private RequestDelegate buildRequestDelegate(IServer server)
        {
            // TODO -- don't do this if this is bootstrapped through WebHostBuilder

            var factory = new ApplicationBuilderFactory(Container);
            var builder = factory.CreateBuilder(server.Features);
            builder.ApplicationServices = Container;


            // TODO -- use QuickBuild on this
            var startupFilters = Container.GetAllInstances<IStartupFilter>();


            // Hate, hate, hate this code, but I blame the ASP.Net team
            // some how
            Action<IApplicationBuilder> configure = app =>
            {
                configureAppBuilder(app, out var startups);
                if (startups.Any())
                {
                    var services = new ServiceCollection();
                    foreach (var startup in startups)
                    {
                        startup.ConfigureServices(services);
                    }

                    Container.Configure(services);
                }
            };

            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            configure(builder);


            return builder.Build();
        }

        private void configureAppBuilder(IApplicationBuilder app, out IStartup[] startups)
        {
            var router = Registry.HttpRoutes.Routes.Router;
            app.StoreRouter(router);

            startups = Container.GetAllInstances<IStartup>().ToArray();

            foreach (var startup in startups)
            {
                startup.Configure(app);
            }

            if (!app.HasJasperBeenApplied() && router.HasAnyRoutes())
            {
                app.Run(router.Invoke);
            }
        }

        private IServer buildServer()
        {
            var server = Container.TryGetInstance<IServer>();
            if (server == null)
            {
                return new NulloServer();
            }

            // TODO -- probably need to test this
            var serverAddressesFeature = server.Features?.Get<IServerAddressesFeature>();
            var addresses = serverAddressesFeature?.Addresses;
            if (addresses != null && !addresses.IsReadOnly && addresses.Count == 0)
            {
                var urls = Registry.EnvironmentConfiguration[WebHostDefaults.ServerUrlsKey] ;
                if (!string.IsNullOrEmpty(urls))
                {
                    serverAddressesFeature.PreferHostingUrls = WebHostUtilities.ParseBool(Registry.EnvironmentConfiguration, WebHostDefaults.PreferHostingUrlsKey);

                    foreach (var value in urls.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        addresses.Add(value);
                    }
                }
            }

            return server;
        }


    }
}
