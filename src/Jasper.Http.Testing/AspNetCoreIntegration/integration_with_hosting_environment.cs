using System.Threading.Tasks;
using Jasper.Http.Testing.ContentHandling;
using Jasper.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Shouldly;
using Xunit;

namespace Jasper.Http.Testing.AspNetCoreIntegration
{
    public class integration_with_hosting_environment
    {
        [Fact]
        public void IHostingEnvironment_is_in_container()
        {
            HttpTesting.Runtime.Get<IHostingEnvironment>()
                .ShouldBeOfType<HostingEnvironment>();
        }

        [Fact]
        public async Task hosting_environment_uses_config()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Hosting.UseEnvironment("Fake");

            var runtime = await JasperRuntime.ForAsync(registry);

            try
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task hosting_environment_uses_config_2()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Hosting.UseEnvironment("Fake2");

            var runtime = await JasperRuntime.ForAsync(registry);

            try
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake2");
            }
            finally
            {
                await runtime.Shutdown();
            }
        }


        public class FakeSettings
        {
            public string Environment { get; set; }
        }
    }
}
