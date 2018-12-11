using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.Http;
using Jasper.Http.Routing;
using Jasper.Messaging;
using Lamar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper
{
    public static class WebHostBuilderExtensions
    {
        public static IWebHostBuilder UseJasper<T>(this IWebHostBuilder builder, Action<T> overrides = null) where T : JasperRegistry, new ()
        {
            var registry = new T();
            overrides?.Invoke(registry);
            return builder.UseJasper(registry);
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder, JasperRegistry registry)
        {
            JasperRuntime.ApplyExtensions(registry);

            registry.HttpRoutes.StartFindingRoutes(registry.ApplicationAssembly);
            registry.Messaging.StartCompiling(registry);

            registry.Settings.Apply(registry.Services);

            builder.ConfigureServices(s =>
            {
                s.AddSingleton<IHostedService, JasperActivator>();

                s.AddRange(registry.CombineServices());
                s.AddSingleton(registry);

                s.AddSingleton<IServiceProviderFactory<ServiceRegistry>, LamarServiceProviderFactory>();
                s.AddSingleton<IServiceProviderFactory<IServiceCollection>, LamarServiceProviderFactory>();

                s.AddSingleton<IStartupFilter>(new RegisterJasperStartupFilter());
            });

            return builder;
        }

        public static IWebHostBuilder UseJasper(this IWebHostBuilder builder)
        {
            return builder.UseJasper(new JasperRegistry());
        }

        public const string JasperRouterKey = "JasperRouter";
        public static readonly string JasperHasBeenApplied = "JasperHasBeenApplied";

        public static IApplicationBuilder UseJasper(this IApplicationBuilder app)
        {
            if (app.HasJasperBeenApplied())
                throw new InvalidOperationException("Jasper has already been applied to this web application");

            var router = app.Properties.ContainsKey(JasperRouterKey)
                ? app.Properties[JasperRouterKey].As<Router>()
                : app.ApplicationServices.GetRequiredService<Router>();

            app.MarkJasperHasBeenApplied();

            return app.Use(next => c => router.Invoke(c, next));
        }

        internal static void StoreRouter(this IApplicationBuilder builder, Router router)
        {
            builder.Properties.Add(JasperRouterKey, router);
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
    }

    internal class RegisterJasperStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);
                if (!app.HasJasperBeenApplied())
                {
                    var router = app.Properties.ContainsKey(WebHostBuilderExtensions.JasperRouterKey)
                        ? app.Properties[WebHostBuilderExtensions.JasperRouterKey].As<Router>()
                        : app.ApplicationServices.GetRequiredService<Router>();

                    app.MarkJasperHasBeenApplied();

                    app.Run(router.Invoke);
                }
            };

        }
    }

    internal class JasperActivator : IHostedService
    {
        private readonly JasperRegistry _registry;
        private readonly IMessagingRoot _root;
        private readonly IContainer _container;

        public JasperActivator(JasperRegistry registry, IMessagingRoot root, IContainer container)
        {
            _registry = registry;
            _root = root;
            _container = container;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await _registry.Messaging.Compiling;
            await _registry.HttpRoutes.BuildRouting(_container, _registry.CodeGeneration);

            _root.Activate(_registry.Messaging.LocalWorker, _registry.CodeGeneration, _container);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
