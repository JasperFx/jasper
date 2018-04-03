using System;
using Jasper.Messaging.Model;
using Jasper.Messaging.Sagas;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging.Sagas
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

        [Fact]
        public void is_saga_related_false()
        {
            var chain = HandlerChain.For<FooHandler>(x => x.Handle(null));
            SagaFramePolicy.IsSagaRelated(chain).ShouldBeFalse();
        }

        [Fact]
        public void is_saga_related_true()
        {
            var chain = HandlerChain.For<FooSaga>(x => x.Handle(null));
            SagaFramePolicy.IsSagaRelated(chain).ShouldBeTrue();
        }


    }

    public class FooSaga : StatefulSagaOf<FooState>
    {
        public void Handle(WithIdProp prop)
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

        [SagaIdentity]
        public string Name { get; set; }
    }
}
