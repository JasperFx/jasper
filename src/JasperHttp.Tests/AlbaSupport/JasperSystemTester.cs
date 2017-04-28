using System;
using System.Threading.Tasks;
using Alba;
using AlbaForJasper;
using Baseline;
using Jasper;
using Jasper.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using StructureMap;
using Xunit;

namespace JasperHttp.Tests.AlbaSupport
{
    public class JasperSystemTester : IDisposable
    {
        public void Dispose()
        {
            theSystem?.Dispose();
        }

        private readonly JasperSystem<AlbaTargetApp> theSystem = JasperSystem.For<AlbaTargetApp>();

        [Fact]
        public async Task run_simple_scenario_that_uses_custom_services()
        {
            var startup = theSystem.Services.GetService<IStartup>();
            var factory = theSystem.Services.GetService<IServiceProviderFactory<IServiceCollection>>();


            theSystem.Services.ShouldBeOfType<StructureMapServiceProvider>();

            

            await theSystem.Scenario(_ =>
            {
                _.Get.Url("/");
                _.StatusCodeShouldBeOk();
                _.ContentShouldContain("Texas");
            });
        }
    }

    internal class AlbaTargetAppStartup
    {
//        public void ConfigureServices(IServiceCollection services)
//        {
//        }
//
//        public void ConfigureContainer(IContainer container)
//        {
//        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Run(c =>
            {
                var settings = c.RequestServices.GetService(typeof(SomeSettings)).As<SomeSettings>();
                c.Response.StatusCode = 200;
                return c.Response.WriteAsync(settings.Name);
            });
        }
    }

    public class AlbaTargetApp : JasperRegistry
    {
        public AlbaTargetApp()
        {
            var feature = Feature<AspNetCoreFeature>();

            feature.Host.Configure(app =>
            {
                app.Run(c =>
                {
                    var settings = c.RequestServices.GetService(typeof(SomeSettings)).As<SomeSettings>();
                    c.Response.StatusCode = 200;
                    return c.Response.WriteAsync(settings.Name);
                });
            });



            //feature.Host.UseStartup<AlbaTargetAppStartup>();

            Services.For<SomeSettings>().Use(new SomeSettings {Name = "Texas"});
            feature.Host.UseKestrel();
            feature.Host.UseUrls("http://localhost:5555");

        }
    }



    public class SomeSettings
    {
        public string Name { get; set; }
    }
}