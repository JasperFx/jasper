using System.Linq;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Shouldly;
using StructureMap;
using Xunit;

namespace Jasper.Testing.Codegen.IoC
{
    public class StructureMapServicesTests
    {
        private readonly IContainer theContainer = new Container();
        private StructureMapServices theServices;

        public StructureMapServicesTests()
        {
            theServices = new StructureMapServices(theContainer);
        }

        [Fact]
        public void return_a_nested_container_variable_for_IContainer()
        {
            theServices.Matches(typeof(IContainer)).ShouldBeTrue();

            theServices.Create(typeof(IContainer))
                .ShouldBeSameAs(StructureMapServices.Nested);
        }

        [Fact]
        public void nested_variable_has_the_root_container_as_a_dependent_variable()
        {
            var field = StructureMapServices.Nested.Dependencies.Single()
                .ShouldBeOfType<InjectedField>();

            field.ArgType.ShouldBe(typeof(IContainer));
            field.CtorArg.ShouldBe("root");
        }

        [Fact]
        public void do_not_match_any_kind_of_simple_type()
        {
            theServices.Matches(typeof(int)).ShouldBeFalse();
            theServices.Matches(typeof(string)).ShouldBeFalse();
        }

        [Fact]
        public void does_match_a_registered_singleton()
        {
            theContainer.Configure(_ =>
            {
                _.ForConcreteType<SingletonService>()
                    .Configure.Singleton();
            });

            theServices.Matches(typeof(SingletonService))
                .ShouldBeTrue();

            theServices.Create(typeof(SingletonService))
                .ShouldBeOfType<InjectedField>()
                .VariableType
                .ShouldBe(typeof(SingletonService));
        }


        [Fact]
        public void returns_a_service_variable_when_there_is_a_registration()
        {
            theContainer.Configure(_ =>
            {
                _.ForConcreteType<ScopedService>()
                    .Configure.Transient();
            });

            theServices.Matches(typeof(ScopedService))
                .ShouldBeTrue();

            theServices.Create(typeof(ScopedService))
                .ShouldBeOfType<ServiceVariable>()
                .VariableType.ShouldBe(typeof(ScopedService));
        }

        [Fact]
        public void service_variable_exposes_the_nested_container_as_a_parent()
        {
            var variable = new ServiceVariable(typeof(ScopedService), StructureMapServices.Nested);

            variable.Dependencies.Single()
                .ShouldBeSameAs(StructureMapServices.Nested);
        }

        public class ScopedService
        {

        }

        public class SingletonService
        {

        }
    }
}