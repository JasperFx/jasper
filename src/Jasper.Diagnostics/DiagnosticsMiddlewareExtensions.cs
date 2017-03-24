using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders.Embedded;
using JasperBus;
using Jasper.Remotes.Messaging;

namespace Jasper.Diagnostics
{
    public static class DiagnosticsMiddlewareExtensions
    {
        public static IApplicationBuilder UseDiagnostics(
            this IApplicationBuilder app,
            Action<DiagnosticsSettings> configure = null)
        {
            var options = new DiagnosticsSettings();
            configure?.Invoke(options);
            return UseDiagnostics(app, options);
        }

        public static IApplicationBuilder UseDiagnostics(
            this IApplicationBuilder app,
            DiagnosticsSettings options)
        {
            app.UseWebSockets();

            if(options.Mode == DiagnosticsMode.Production)
            {
                var assembly = typeof(DiagnosticsMiddleware).GetTypeInfo().Assembly;
                var provider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.resources");

                app.UseStaticFiles(new StaticFileOptions()
                {
                    FileProvider = provider,
                    RequestPath = new PathString($"{options.BasePath}{DiagnosticsMiddleware.Resource_Root}")
                });
            }

            var hub = app.ApplicationServices.GetService<IMessagingHub>();
            var manager = app.ApplicationServices.GetService<ISocketConnectionManager>();

            app.MapWebSocket($"{options.BasePath}/ws",
                new SocketConnection((socket, text) => {
                    Console.WriteLine("Socket: {0}", text);
                    hub.SendJson(text);
                    return Task.CompletedTask;
                }),
                manager);

            app.UseMiddleware<DiagnosticsMiddleware>(options);

            return app;
        }

        public static JasperRegistry AddDiagnostics(this JasperRegistry registry)
        {
            registry.Logging.LogBusEventsWith<DiagnosticsBusLogger>();
            registry.Services.IncludeRegistry<DiagnosticServicesRegistry>();

            return registry;
        }
    }
}
