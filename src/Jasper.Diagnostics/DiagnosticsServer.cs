using System;
using System.IO;
using BlueMilk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jasper.Diagnostics
{
    internal class DiagnosticsServerStartup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void ConfigureContainer(IContainer container)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            var settings = app.ApplicationServices.GetService<DiagnosticsSettings>();
            app.UseDiagnostics(settings);

            app.Run(async http =>
            {
                http.Response.StatusCode = 200;
                http.Response.ContentType = "text/plain";
                await http.Response.WriteAsync($"Nothing to see here at {DateTime.Now}.");
            });
        }
    }

}
