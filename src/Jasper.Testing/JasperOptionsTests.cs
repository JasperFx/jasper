using Jasper.Configuration;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using TestingSupport.Fakes;
using Xunit;

namespace Jasper.Testing
{
    public class JasperOptionsTests
    {
        private readonly JasperOptions theSettings = new JasperOptions();

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        public class MyOptions : JasperOptions
        {
        }


        [Fact]
        public void unique_node_id_is_really_unique()
        {
            var options1 = new AdvancedSettings();
            var options2 = new AdvancedSettings();
            var options3 = new AdvancedSettings();
            var options4 = new AdvancedSettings();
            var options5 = new AdvancedSettings();
            var options6 = new AdvancedSettings();

            options1.UniqueNodeId.ShouldNotBe(options2.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options1.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options2.UniqueNodeId.ShouldNotBe(options3.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options2.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options3.UniqueNodeId.ShouldNotBe(options4.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options3.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options4.UniqueNodeId.ShouldNotBe(options5.UniqueNodeId);
            options4.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);

            options5.UniqueNodeId.ShouldNotBe(options6.UniqueNodeId);
        }

        [Fact]
        public void can_determine_the_root_assembly_on_subclass()
        {
            new MyOptions().ApplicationAssembly.ShouldBe(typeof(JasperOptionsTests).Assembly);
        }

        [Fact]
        public void sets_up_the_container_with_services()
        {
            var registry = new JasperOptions();
            registry.Handlers.DisableConventionalDiscovery();
            registry.Services.For<IFoo>().Use<Foo>();
            registry.Services.AddTransient<IFakeStore, FakeStore>();

            using (var runtime = JasperHost.For(registry))
            {
                runtime.Get<IContainer>().DefaultRegistrationIs<IFoo, Foo>();
            }
        }
    }
}
