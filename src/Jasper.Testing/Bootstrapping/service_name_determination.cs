using Shouldly;
using Xunit;

namespace Jasper.Testing.Bootstrapping
{
    public class service_name_determination
    {
        [Fact]
        public void determine_name_from_JasperRegistry_type_name()
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
