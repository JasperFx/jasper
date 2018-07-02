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
using IApplicationLifetime = Microsoft.AspNetCore.Hosting.IApplicationLifetime;
using TypeExtensions = Baseline.TypeExtensions;

namespace Jasper
{
    public partial class JasperRuntime
    {
        private IHostedService[] _hostedServices;
        private IServer _server;
        private ApplicationLifetime _applicationLifetime;
        private ILogger _logger;

        private async Task startHostedServices()
        {
            if (!Registry.MessagingSettings.HostedServicesEnabled) return;

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
                return timer.Record("Combining Services and Building Settings", registry.CompileConfigurationAndServicesForIdiomaticBootstrapping);
            });



            var handlerCompilation = registry.Messaging.CompileHandlers(registry, timer);


            var runtime = new JasperRuntime(registry, timer);

            var services = await buildingServices;
            services.AddSingleton(runtime);


            var container = await Lamar.Container.BuildAsync(services, timer);
            container.DisposalLock = DisposalLock.Ignore;
            runtime.Container = container;

            var routeDiscovery = registry.HttpRoutes.Enabled
                ? registry.HttpRoutes.FindRoutes(runtime, registry, timer)
                : Task.CompletedTask;

            runtime.buildAspNetCoreServer(services);

            await routeDiscovery;

            await Task.WhenAll(runtime.startAspNetCoreServer(), handlerCompilation, runtime.startHostedServices());


            // Run environment checks
            timer.Record("Environment Checks", () =>
            {
                var recorder = EnvironmentChecker.ExecuteAll(runtime);
                if (registry.MessagingSettings.ThrowOnValidationErrors) recorder.AssertAllSuccessful();
            });

            _lifetime = TypeExtensions.As<ApplicationLifetime>(container.GetInstance<IApplicationLifetime>());
            _lifetime.NotifyStarted();

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
                .Select(x => TypeExtensions.As<IJasperExtension>(Activator.CreateInstance(x)))
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
            await _server.StopAsync(Registry.MessagingSettings.Cancellation).ConfigureAwait(false);

            // Fire IApplicationLifetime.Stopped
            _applicationLifetime?.NotifyStopped();

            HostingEventSource.Log.HostStop();
        }

        public RequestDelegate RequestDelegate { get; private set; }

        private async Task startAspNetCoreServer()
        {
            if (!Registry.HttpRoutes.Enabled || Registry.BootstrappedWithinAspNetCore) return;


            HostingEventSource.Log.HostStart();

            _logger = Get<ILoggerFactory>().CreateLogger("Jasper");

            _applicationLifetime = TypeExtensions.As<ApplicationLifetime>(Container.GetInstance<IApplicationLifetime>());

            var httpContextFactory = Container.QuickBuild<HttpContextFactory>();

            var hostingApp = new HostingApplication(
                RequestDelegate,
                _logger,
                Container.GetInstance<DiagnosticListener>(), // See if this can be passed in directly
                httpContextFactory);

            await _server.StartAsync(hostingApp, Registry.MessagingSettings.Cancellation);

            // Fire IApplicationLifetime.Started
            _applicationLifetime?.NotifyStarted();
        }

        private void buildAspNetCoreServer(ServiceRegistry originalServices)
        {
            _server = buildServer();

            RequestDelegate = buildRequestDelegate(_server, originalServices);

            HttpAddresses = Registry.HttpAddresses?.Split(';').ToArray() ??
                            new string[0];

        }

        private RequestDelegate buildRequestDelegate(IServer server, ServiceRegistry originalServices)
        {
            var factory = new ApplicationBuilderFactory(Container);
            var builder = factory.CreateBuilder(server.Features);
            builder.ApplicationServices = Container;


            var startupFilters = Container.QuickBuildAll<IStartupFilter>();


            // Hate, hate, hate this code, but I blame the ASP.Net team
            // some how
            Action<IApplicationBuilder> configure = app => configureAppBuilder(app, originalServices);

            foreach (var filter in startupFilters.Reverse())
            {
                configure = filter.Configure(configure);
            }

            configure(builder);


            return builder.Build();
        }

        private void configureAppBuilder(IApplicationBuilder app, ServiceRegistry originalServices)
        {
            var router = Registry.HttpRoutes.Routes.Router;
            app.StoreRouter(router);

            var startups = Container.QuickBuildAll<IStartup>().ToArray();

            if (startups.Any())
            {
                var services = new ServiceCollection();

                // MVC wants to reach into the ServiceCollection to pick out
                // things like the IHostedEnvironment, so we have to temporarily
                // copy stuff in, then pull it out before it gets double registered
                services.AddRange(originalServices);

                foreach (var startup in startups)
                {
                    startup.ConfigureServices(services);
                }

                // See snarky comment above
                services.RemoveAll(originalServices.Contains);

                Container.Configure(services);
            }

            foreach (var startup in startups)
            {
                startup.Configure(app);
            }

            // There's a race condition now between the router being completely "found"
            // and this, so we no longer check for the existence of any routes
            if (!app.HasJasperBeenApplied())
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
