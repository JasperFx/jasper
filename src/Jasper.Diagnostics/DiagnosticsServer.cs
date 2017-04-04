using System;
using System.IO;
using Jasper.Diagnostics.StructureMap;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

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

    public class DiagnosticsServer : IDisposable
    {
        private IWebHost _host;
        private DiagnosticsSettings _settings;

        public void Start(DiagnosticsSettings settings, IContainer container)
        {
            _settings = settings;

            var url = $"http://localhost:{settings.WebsocketPort}";
            Console.WriteLine($"Diagnostics listening on {url}");
            _host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls(url)
                .UseStructureMap(container)
                .UseStartup<DiagnosticsServerStartup>()
                .Build();

            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
