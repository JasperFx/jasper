using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Http.AspNetCoreIntegration
{
    public class integration_with_hosting_environment
    {
        public class FakeSettings
        {
            public string Environment { get; set; }
        }

        public class MySpecialRegistry : JasperRegistry
        {
            public MySpecialRegistry()
            {
                Handlers.DisableConventionalDiscovery();
                ServiceName = "MySpecialApp";
            }
        }

        [Fact]
        public async Task hosting_environment_app_name_is_application_assembly_name()
        {
            using (var runtime = JasperRuntime.For<MySpecialRegistry>())

            {
                // This is important for the MVC and ASP.Net Core integration to work correctly
                runtime.Get<IHostingEnvironment>().ApplicationName.ShouldBe(Assembly.GetExecutingAssembly().FullName);
            }

        }

        [Fact]
        public async Task hosting_environment_uses_config()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Hosting.UseEnvironment("Fake");

            using (var runtime = await JasperRuntime.ForAsync(registry))
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake");
            }

        }

        [Fact]
        public async Task hosting_environment_uses_config_2()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Hosting.UseEnvironment("Fake2");

            using (var runtime = await JasperRuntime.ForAsync(registry))
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake2");
            }

        }
    }
}
