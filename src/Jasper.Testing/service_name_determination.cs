using Shouldly;
using Xunit;

namespace Jasper.Testing
{
    public class service_name_determination
    {
        [Fact]
        public void determine_name_from_JasperRegistry_type_name()
        {
            new MyAppJasperRegistry().ServiceName.ShouldBe("MyApp");
            new MyApp2Registry().ServiceName.ShouldBe("MyApp2");
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
            new JasperRegistry().ServiceName.ShouldBe("Jasper.Testing");
        }
    }

    public class MyAppJasperRegistry : JasperRegistry
    {

    }

    public class MyApp2Registry : JasperRegistry
    {

    }

    public class MyApp3 : JasperRegistry
    {

    }

    public class MyApp4 : JasperRegistry
    {
        public MyApp4()
        {
            ServiceName = "SomethingService";
        }
    }
}
