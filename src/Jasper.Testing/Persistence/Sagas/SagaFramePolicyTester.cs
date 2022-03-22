using System;
using Jasper.Attributes;
using Jasper.Persistence.Sagas;
using Jasper.Runtime.Handlers;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Persistence.Sagas
{
    public class SagaFramePolicyTester
    {
        [Theory]
        [InlineData(typeof(WithIdProp), "SagaId")]
        [InlineData(typeof(WithAttribute), "Name")]
        public void choose_saga_id_prop(Type messageType, string propertyName)
        {
            SagaFramePolicy.ChooseSagaIdProperty(messageType)
                .Name.ShouldBe(propertyName);
        }

        [Theory]
        [InlineData("Handle", SagaStateExistence.Existing)]
        [InlineData("Start", SagaStateExistence.New)]
        [InlineData("Starts", SagaStateExistence.New)]
        [InlineData("Orchestrates", SagaStateExistence.Existing)]
        public void determine_saga_existence_from_handler_call(string methodName, SagaStateExistence existence)
        {
            var method = typeof(FooSaga).GetMethod(methodName);
            var call = new HandlerCall(typeof(FooSaga), method);

            SagaFramePolicy.DetermineExistence(call)
                .ShouldBe(existence);
        }

        [Fact]
        public void determine_the_saga_state_type()
        {
            var chain = HandlerChain.For<FooSaga>(x => x.Handle(null), null);
            SagaFramePolicy.DetermineSagaStateType(chain)
                .ShouldBe(typeof(FooState));
        }

        [Fact]
        public void determine_the_saga_state_type_with_multiple_levels_of_abstraction()
        {
            var chain = HandlerChain.For<DoubleInherited>(x => x.Handle(null), null);
            SagaFramePolicy.DetermineSagaStateType(chain)
                .ShouldBe(typeof(FooState));
        }

        [Fact]
        public void is_saga_related_false()
        {
            var chain = HandlerChain.For<FooHandler>(x => x.Handle(null), null);
            SagaFramePolicy.IsSagaRelated(chain).ShouldBeFalse();
        }

        [Fact]
        public void is_saga_related_true()
        {
            var chain = HandlerChain.For<FooSaga>(x => x.Handle(null), null);
            SagaFramePolicy.IsSagaRelated(chain).ShouldBeTrue();
        }
    }

    [JasperIgnore]
    public class FooSaga : StatefulSagaOf<FooState>
    {
        public void Handle(WithIdProp prop)
        {
        }

        public void Start(WithIdProp prop)
        {
        }

        public void Starts(WithIdProp prop)
        {
        }

        public void Orchestrates(WithIdProp prop)
        {
        }
    }

    public class FooState
    {
        public Guid Id { get; set; }
    }

    public class FooHandler
    {
        public void Handle(WithIdProp prop)
        {
        }
    }

    public class InvalidPropType
    {
        public DateTime SagaId { get; set; }
    }

    public class WithIdProp
    {
        public string SagaId { get; set; }

        public string Name { get; set; }
    }

    public class WithAttribute
    {
        public string SagaId { get; set; }

        [SagaIdentity] public string Name { get; set; }
    }

    public abstract class AbstractSaga<T> : StatefulSagaOf<T>
    {
    }

    [JasperIgnore]
    public class DoubleInherited : AbstractSaga<FooState>
    {
        public void Handle(WithIdProp prop)
        {
        }
    }
}
