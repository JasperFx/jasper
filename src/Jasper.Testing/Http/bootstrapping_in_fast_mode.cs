using System;
using System.Linq;
using System.Threading.Tasks;
using Alba;
using Jasper.Http;
using Lamar;
using Marten;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http
{
    public class bootstrapping_in_fast_mode
    {
        [Fact]
        public void should_be_auto_request_filter_present_in_fully_compliant_mode()
        {
            using (var runtime = JasperRuntime.For(x => x.HttpRoutes.AspNetCoreCompliance = ComplianceMode.FullyCompliant))
            {
                runtime.Container.Model.For<IStartupFilter>().Instances
                    .Any(x => x.ImplementationType == typeof(AutoRequestServicesStartupFilter))
                    .ShouldBeTrue();
            }
        }

        [Fact]
        public void should_not_be_any_auto_request_filter()
        {
            using (var runtime = JasperRuntime.For(x => x.HttpRoutes.AspNetCoreCompliance = ComplianceMode.GoFaster))
            {
                runtime.Container.Model.For<IStartupFilter>().Instances
                    .Any(x => x.ImplementationType == typeof(AutoRequestServicesStartupFilter))
                    .ShouldBeFalse();
            }
        }

        [Fact]
        public async Task run_end_to_end()
        {
            using (var system = SystemUnderTest.For(x =>
            {
                x
                    .UseStartup<EmptyStartup>()
                    .UseJasper(o =>
                    {
                        o.HttpRoutes.AspNetCoreCompliance = ComplianceMode.GoFaster;
                    });
            }))
            {
                await system.Scenario(x =>
                {
                    x.Get.Url("/fast/text");
                    x.ContentShouldBe("some fast text");
                });
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
                var runtime = JasperRuntime.For(x =>
                {
                    x.HttpRoutes.AspNetCoreCompliance = ComplianceMode.GoFaster;
                });

                // Or the ASP.Net Core way
                var host = WebHost.CreateDefaultBuilder()
                    .UseStartup<Startup>()
                    .UseJasper(x => x.HttpRoutes.AspNetCoreCompliance = ComplianceMode.GoFaster)
                    .Start();
                // ENDSAMPLE
            }
        }
    }
}
