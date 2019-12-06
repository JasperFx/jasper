using System.Linq;
using Jasper.Configuration;
using Lamar;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class including_extensions
    {
        [Fact]
        public void the_application_still_wins()
        {
            var registry = new JasperOptions();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Extensions.Include<OptionalExtension>();
            registry.Services.For<IColorService>().Use<BlueService>();

            using (var runtime = JasperHost.For(registry))
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<BlueService>();
            }
        }

        [Fact]
        public void will_apply_an_extension()
        {
            // SAMPLE: explicitly-add-extension
            var registry = new JasperOptions();
            registry.Extensions.Include<OptionalExtension>();
            // ENDSAMPLE

            registry.Handlers.DisableConventionalDiscovery();

            using (var runtime = JasperHost.For(registry))
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<RedService>();
            }
        }

        [Fact]
        public void will_only_apply_extension_once()
        {
            var registry = new JasperOptions();
            registry.Extensions.Include<OptionalExtension>();
            registry.Extensions.Include<OptionalExtension>();
            registry.Extensions.Include<OptionalExtension>();
            registry.Extensions.Include<OptionalExtension>();

            using (var host = JasperHost.For(registry))
            {
                host.Get<IContainer>().Model.For<IColorService>().Instances
                    .Count().ShouldBe(1);
            }
        }
    }

    public interface IColorService
    {
    }

    public class RedService : IColorService
    {
    }

    public class BlueService : IColorService
    {
    }

    public class OptionalExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Services.For<IColorService>().Use<RedService>();
        }
    }
}
