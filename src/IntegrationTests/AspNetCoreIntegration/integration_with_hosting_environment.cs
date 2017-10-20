﻿using Jasper;
using Jasper.Testing.Http.ContentHandling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Shouldly;
using Xunit;

namespace IntegrationTests.AspNetCoreIntegration
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
        public void hosting_environment_uses_config()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.EnvironmentName = "Fake";

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake");
            }
        }

        [Fact]
        public void hosting_environment_uses_config_2()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Http.UseEnvironment("Fake2");

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake2");
            }
        }


        public class FakeSettings
        {
            public string Environment { get; set; }
        }
    }
}
