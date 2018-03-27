using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class including_extensions
    {
        [Fact]
        public void will_apply_an_extension()
        {
            var registry = new JasperRegistry();
            registry.Include<OptionalExtension>();
            registry.Handlers.DisableConventionalDiscovery(true);

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<RedService>();
            }
        }

        [Fact]
        public void the_application_still_wins()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Include<OptionalExtension>();
            registry.Services.For<IColorService>().Use<BlueService>();

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<BlueService>();
            }
        }
    }

    public interface IColorService{}
    public class RedService : IColorService{}
    public class BlueService : IColorService{}

    public class OptionalExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.For<IColorService>().Use<RedService>();
        }
    }
}
