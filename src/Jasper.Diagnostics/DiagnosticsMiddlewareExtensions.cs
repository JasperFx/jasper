using System;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders.Embedded;
using Jasper.WebSockets;

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
            app.UseJasperWebSockets();

            var assembly = typeof(DiagnosticsMiddleware).GetTypeInfo().Assembly;
            var provider = new EmbeddedFileProvider(assembly, $"{assembly.GetName().Name}.resources");

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = provider,
                RequestPath = options.BasePath
            });

            app.UseMiddleware<DiagnosticsMiddleware>(options);

            return app;
        }
    }
}
