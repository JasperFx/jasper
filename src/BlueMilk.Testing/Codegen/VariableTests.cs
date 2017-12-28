using System.Collections.Generic;
using Jasper.Internals.Codegen;
using Jasper.Testing.Internals.TargetTypes;
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
        public void default_arg_name_of_array()
        {
            Variable.DefaultArgName<IWidget[]>()
                .ShouldBe("widgetArray");
        }

        [Fact]
        public void default_arg_name_of_List()
        {
            Variable.DefaultArgName<IList<IWidget>>()
                .ShouldBe("widgetList");

            Variable.DefaultArgName<List<IWidget>>()
                .ShouldBe("widgetList");

            Variable.DefaultArgName<IReadOnlyList<IWidget>>()
                .ShouldBe("widgetList");
        }

        [Fact]
        public void default_arg_name_of_enumerable()
        {
            Variable.DefaultArgName<IEnumerable<IWidget>>()
                .ShouldBe("widgetEnumerable");
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
