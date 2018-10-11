using System.Threading.Tasks;
using Jasper.Configuration;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class including_extensions
    {
        [Fact]
        public async Task will_apply_an_extension()
        {
            // SAMPLE: explicitly-add-extension
            var registry = new JasperRegistry();
            registry.Include<OptionalExtension>();
            // ENDSAMPLE

            registry.Handlers.DisableConventionalDiscovery(true);

            var runtime = await JasperRuntime.ForAsync(registry);

            try
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<RedService>();
            }
            finally
            {
                await runtime.Shutdown();
            }
        }

        [Fact]
        public async Task the_application_still_wins()
        {
            var registry = new JasperRegistry();
            registry.Handlers.DisableConventionalDiscovery(true);
            registry.Include<OptionalExtension>();
            registry.Services.For<IColorService>().Use<BlueService>();

            var runtime = await JasperRuntime.ForAsync(registry);

            try
            {
                runtime.Get<IColorService>()
                    .ShouldBeOfType<BlueService>();
            }
            finally
            {
                await runtime.Shutdown();
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
