using System.Linq;
using Jasper.CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;
using Shouldly;
using Xunit;

namespace Jasper.Testing.CommandLine
{
    public class JasperInputTests
    {
        [Fact]
        public void pass_the_environment_flag()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Http.Actions.DisableConventionalDiscovery();

            var input = new JasperInput
            {
                Registry = registry,
                EnvironmentFlag = "Fake"
            };

            using (var runtime = input.BuildRuntime())
            {
                runtime.Get<IHostingEnvironment>()
                    .EnvironmentName.ShouldBe("Fake");
            }
        }

        [Fact]
        public void set_up_verbose_logging()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Http.Actions.DisableConventionalDiscovery();

            var input = new JasperInput
            {
                Registry = registry,
                VerboseFlag = true
            };

            using (var runtime = input.BuildRuntime())
            {
                var providers = runtime.Container.GetAllInstances<ILoggerProvider>();
                providers.OfType<ConsoleLoggerProvider>().Any().ShouldBeTrue();
                providers.OfType<DebugLoggerProvider>().Any().ShouldBeTrue();

            }
        }
    }
}
