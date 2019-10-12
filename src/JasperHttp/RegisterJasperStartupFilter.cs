using System;
using JasperHttp.Routing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JasperHttp
{
    internal class RegisterJasperStartupFilter : IStartupFilter
    {
        private readonly JasperHttpOptions _options;

        public RegisterJasperStartupFilter(JasperHttpOptions options)
        {
            _options = options;
        }

        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                var httpSettings = app.ApplicationServices.GetRequiredService<JasperHttpOptions>();
                if (!httpSettings.Enabled)
                {
                    next(app);
                    return;
                }

                var logger = app.ApplicationServices.GetRequiredService<ILogger<JasperHttpOptions>>();

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
                            logger.LogError(e,
                                $"Failed during an HTTP request for {c.Request.Method}: {c.Request.Path}");
                            c.Response.StatusCode = 500;
                            return c.Response.WriteAsync(e.ToString());
                        }
                    };
                });
                next(app);
                if (!app.HasJasperBeenApplied())
                    Router.BuildOut(app).Run(c =>
                    {
                        c.Response.StatusCode = 404;
                        c.Response.Headers["status-description"] = "Resource Not Found";
                        return c.Response.WriteAsync("Resource Not Found");
                    });
            };
        }
    }
}
