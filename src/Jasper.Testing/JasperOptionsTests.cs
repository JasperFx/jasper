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
            var options1 = new AdvancedSettings(null);
            var options2 = new AdvancedSettings(null);
            var options3 = new AdvancedSettings(null);
            var options4 = new AdvancedSettings(null);
            var options5 = new AdvancedSettings(null);
            var options6 = new AdvancedSettings(null);

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

        [Fact]
        public void stub_out_external_setting_via_IEndpoints()
        {
            var options = new JasperOptions();
            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeFalse();

            options.Endpoints.StubAllExternallyOutgoingEndpoints();

            options.Advanced.StubAllOutgoingExternalSenders.ShouldBeTrue();
        }

        [Fact]
        public void determine_name_from_JasperOptions_type_name()
        {
            new MyAppJasperOptions().ServiceName.ShouldBe("MyApp");
            new MyApp2Options().ServiceName.ShouldBe("MyApp2");
            new MyApp3().ServiceName.ShouldBe("MyApp3");
        }

        [Fact]
        public void explicit_service_name_wins()
        {
            new MyApp4().ServiceName.ShouldBe("SomethingService");
        }

        [Fact]
        public void use_the_calling_assembly_name_if_it_is_a_basic_registry()
        {
            new JasperOptions().ServiceName.ShouldBe("Jasper.Testing");
        }
    }

    public class MyAppJasperOptions : JasperOptions
    {
    }

    public class MyApp2Options : JasperOptions
    {
    }

    public class MyApp3 : JasperOptions
    {
    }

    public class MyApp4 : JasperOptions
    {
        public MyApp4()
        {
            ServiceName = "SomethingService";
        }

    }
}
