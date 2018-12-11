using System.Threading.Tasks;
using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class including_extensions
    {
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

        [Fact]
        public void will_apply_an_extension()
        {
            // SAMPLE: explicitly-add-extension
            var registry = new JasperRegistry();
            registry.Include<OptionalExtension>();
            // ENDSAMPLE

            registry.Handlers.DisableConventionalDiscovery(true);

            using (var runtime = JasperRuntime.For(registry))
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<RedService>();
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
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.Services.For<IColorService>().Use<RedService>();
        }
    }
}
