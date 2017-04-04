using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StructureMap;

namespace Jasper.Diagnostics
{
    public static class ServiceCollectionExtensions
    {
        public static IWebHostBuilder UseStructureMap(this IWebHostBuilder builder, IContainer container)
        {
            return builder.ConfigureServices(services => services.AddStructureMap(container));
        }

        public static IServiceCollection AddStructureMap(this IServiceCollection services, IContainer container)
        {
            return services.AddSingleton<IServiceProviderFactory<IContainer>>(new StructureMapServiceProviderFactory(container));
        }
    }

    public class StructureMapServiceProviderFactory : IServiceProviderFactory<IContainer>
    {
        public StructureMapServiceProviderFactory(IContainer container)
        {
            Container = container;
        }

        private IContainer Container { get; }

        public IContainer CreateBuilder(IServiceCollection services)
        {
            var registry = Container ?? new Container();

            registry.Populate(services);

            return registry;
        }

        public IServiceProvider CreateServiceProvider(IContainer container)
        {
            return new StructureMapServiceProvider(container);
        }
    }

    internal class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void ConfigureContainer(IContainer container)
        {
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseDiagnostics(_ =>
            {
                // _.Mode = DiagnosticsMode.Development;
                // _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
                _.WebsocketPort = 5250;
            });

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

        public void Start(int port, IContainer container)
        {
            var hrm = container.WhatDoIHave();
            Console.WriteLine(hrm);

            _host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls($"http://localhost:{port}")
                .UseStructureMap(container)
                .UseStartup<Startup>()
//                .Configure(app =>
//                {
//                    app.UseDiagnostics(_ =>
//                    {
//                        // _.Mode = DiagnosticsMode.Development;
//                        // _.AuthorizeWith = context => context.User.HasClaim("admin", "true");
//                        _.WebsocketPort = port;
//                    });

//                    app.Run(async http =>
//                    {
//                        http.Response.StatusCode = 200;
//                        http.Response.ContentType = "text/plain";
//                        // await http.Response.WriteAsync($"Nothing to see here at {DateTime.Now}.");
//                        await http.Response.WriteAsync(hrm);
//                    });
//                })
                .Build();

            _host.Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}
