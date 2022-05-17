using System.Linq;
using Lamar;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Configuration
{
    public class ExtensionLoadingAndDiscoveryTests : IntegrationContext
    {
        public ExtensionLoadingAndDiscoveryTests(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void the_application_still_wins()
        {
            var registry = new JasperOptions();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Include<OptionalExtension>();
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
            #region sample_explicitly_add_extension
            var registry = new JasperOptions();
            registry.Include<OptionalExtension>();
            #endregion

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
            registry.Include<OptionalExtension>();
            registry.Include<OptionalExtension>();
            registry.Include<OptionalExtension>();
            registry.Include<OptionalExtension>();

            using (var host = JasperHost.For(registry))
            {
                host.Get<IContainer>().Model.For<IColorService>().Instances
                    .Count().ShouldBe(1);
            }
        }

        [Fact]
        public void picks_up_on_handlers_from_extension()
        {
            with(x => x.Include<MyExtension>());

            var handlerChain = chainFor<ExtensionMessage>();
            handlerChain.Handlers.Single()
                .HandlerType.ShouldBe(typeof(ExtensionThing));
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

    public class MyExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Handlers.IncludeType<ExtensionThing>();
        }
    }

    public class ExtensionMessage
    {
    }

    public class ExtensionThing
    {
        public void Handle(ExtensionMessage message)
        {
        }
    }
}
