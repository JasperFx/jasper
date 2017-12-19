using Jasper.Internals.Codegen;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Internals.Codegen
{
    public class VariableTests
    {
        [Fact]
        public void default_arg_name_of_normal_class()
        {
            Variable.DefaultArgName<HyperdriveMotivator>()
                .ShouldBe("hyperdriveMotivator");
        }

        [Fact]
        public void default_arg_name_of_closed_interface()
        {
            Variable.DefaultArgName<IHyperdriveMotivator>()
                .ShouldBe("hyperdriveMotivator");
        }

        [Fact]
        public void default_arg_name_of_generic_class_with_single_parameter()
        {
            Variable.DefaultArgName<FooHandler<HyperdriveMotivator>>()
                .ShouldBe("fooHandler");
        }

        [Fact]
        public void default_arg_name_of_generic_interface_with_single_parameter()
        {
            Variable.DefaultArgName<IFooHandler<HyperdriveMotivator>>()
                .ShouldBe("fooHandler");
        }

        [Fact]
        public void default_arg_name_of_inner_class()
        {
            Variable.DefaultArgName<HyperdriveMotivator.InnerThing>()
                .ShouldBe("innerThing");
        }

        [Fact]
        public void default_arg_name_of_inner_interface()
        {
            Variable.DefaultArgName<HyperdriveMotivator.IInnerThing>()
                .ShouldBe("innerThing");
        }
    }

    public class FooHandler<T>
    {

    }

    public interface IFooHandler<T>
    {

    }

    public interface IHyperdriveMotivator
    {

    }

    public class HyperdriveMotivator
    {
        public class InnerThing
        {

        }

        public interface IInnerThing
        {

        }
    }
}
