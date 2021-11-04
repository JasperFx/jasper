using System;
using Alba;
using Jasper.Http.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Http.Testing.MVCExtensions
{
    public class MvcExtendedApp : IDisposable
    {
        public MvcExtendedApp()
        {
            var builder = Host.CreateDefaultBuilder()
                .UseJasper(x =>
                {
                    x.Extensions.ConfigureHttp(http =>
                    {
                        http.DisableConventionalDiscovery();
                        http.IncludeType<ControllerUsingMvcRouting>();
                        http.IncludeType<ExecutingControllerGuy>();
                        http.IncludeType<TodoController>();
                        http.IncludeType<ControllerUsingJasperRouting>();
                    });
                })
                .ConfigureWebHostDefaults(x => { x.UseStartup<Startup>(); });

            System = new SystemUnderTest(builder);

            Routes = System.Services.GetRequiredService<RouteGraph>();
        }

        public RouteGraph Routes { get; set; }


        public SystemUnderTest System { get; }

        public void Dispose()
        {
            System?.Dispose();
        }
    }
}
