using System;
using System.Linq;
using Baseline;
using BlueMilk.Codegen;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util.StructureMap;
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

            theServices.Create(typeof(IContainer)).ShouldBeOfType<NestedContainerVariable>();
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
        public void can_handle_closed_generic_types()
        {
            var type = typeof(MessageHandler<>).MakeGenericType(typeof(Message1));
            theServices.Matches(type)
                .ShouldBeTrue();
        }

        public class ScopedService
        {

        }

        public class SingletonService
        {

        }

        public class MessageHandler<T>
        {
            public void Handle(T message)
            {

            }
        }
    }
}
