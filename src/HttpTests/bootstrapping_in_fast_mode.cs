using System.Threading.Tasks;
using Alba;
using Jasper;
using JasperHttp;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace HttpTests
{
    public class bootstrapping_in_fast_mode
    {
        public class Startup
        {
            public void ConfigureServices(IServiceCollection services)
            {
            }

            public void Configure(IApplicationBuilder app)
            {
            }
        }


        public class GetFastTextEndpoint
        {
            public string get_fast_text()
            {
                return "some fast text";
            }
        }

        public class ComplianceSample
        {
            public void Go()
            {
                // SAMPLE: GoFasterMode
                // Idiomatic Jasper bootstrapping
                var runtime = JasperHost.For(x =>
                {
                    x.Settings.Http(opts => opts.AspNetCoreCompliance = ComplianceMode.GoFaster);
                });

                // Or the ASP.Net Core way
                var host = WebHost.CreateDefaultBuilder()
                    .UseStartup<Startup>()
                    .UseJasper(x => x.Settings.Http(opts => opts.AspNetCoreCompliance = ComplianceMode.GoFaster))
                    .Start();
                // ENDSAMPLE
            }
        }

        [Fact]
        public async Task run_end_to_end()
        {
            using (var system = SystemUnderTest.For(x =>
            {
                x.UseStartup<Startup>()
                    .UseJasper(o => { o.Settings.Http(opts => opts.AspNetCoreCompliance = ComplianceMode.GoFaster); });
            }))
            {
                await system.Scenario(x =>
                {
                    x.Get.Url("/fast/text");
                    x.ContentShouldBe("some fast text");
                });
            }
        }
    }
}
