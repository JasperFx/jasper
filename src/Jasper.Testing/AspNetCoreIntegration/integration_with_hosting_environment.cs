using System;
using System.Collections.Generic;
using Jasper.Testing.Http.ContentHandling;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.AspNetCoreIntegration
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
            registry.Handlers.ConventionalDiscoveryDisabled = true;
            registry.AspNetCore.UseEnvironment("Fake");

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake");
            }
        }

        [Fact]
        public void can_do_a_with_on_hosting_environment()
        {
            string environment = null;

            var registry = new JasperRegistry();
            registry.Handlers.ConventionalDiscoveryDisabled = true;
            registry.AspNetCore.UseEnvironment("Fake");
            registry.Settings.With<IHostingEnvironment>(_ =>
            {
                throw new Exception("Got here");
                //environment = _.EnvironmentName;
            });

            using (var runtime = JasperRuntime.For(registry))
            {
                environment.ShouldBe("Fake");
            }
        }

        public class FakeSettings
        {
            public string Environment { get; set; }
        }
    }
}
