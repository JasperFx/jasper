using System;
using System.Threading.Tasks;
using Alba;
using Baseline;
using JasperHttpTesting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Jasper.Http.Testing.AlbaSupport
{
    public class JasperSystemTester : IDisposable
    {
        public void Dispose()
        {
            theSystem?.Dispose();
        }

        private ISystemUnderTest theSystem;

        [Fact]
        public async Task run_simple_scenario_that_uses_custom_services()
        {
            theSystem = JasperHttpTester.For<AlbaTargetApp>();

            await theSystem.Scenario(_ =>
            {
                _.Get.Url("/");
                _.StatusCodeShouldBeOk();
                _.ContentShouldContain("Texas");
            });
        }

        [Fact]
        public async Task run_simple_scenario_bootstrapped_by_Startup()
        {
            theSystem = JasperHttpTester.For<AlbaTargetApp2>();

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

    public class AlbaTargetApp : JasperHttpRegistry
    {
        public AlbaTargetApp()
        {
            Http.Configure(app =>
            {
                app.Run(c =>
                {
                    var settings = c.RequestServices.GetService(typeof(SomeSettings)).As<SomeSettings>();
                    c.Response.StatusCode = 200;
                    return c.Response.WriteAsync(settings.Name);
                });
            });



            Services.AddSingleton(new SomeSettings {Name = "Texas"});
            Http.UseKestrel().UseUrls("http://localhost:5555");
        }
    }

    public class AlbaTargetApp2 : JasperHttpRegistry
    {
        public AlbaTargetApp2()
        {
            Http.UseStartup<AlbaTargetAppStartup>();

            Services.AddSingleton(new SomeSettings { Name = "Texas" });
            Http.UseKestrel().UseUrls("http://localhost:5555");
        }
    }


    public class SomeSettings
    {
        public string Name { get; set; }
    }
}
