using System;
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
    }

    public class WithIdProp
    {
        public string SagaId { get; set; }

        public string Name { get; set; }
    }

    public class WithAttribute
    {
        public string SagaId { get; set; }

        [SagaId]
        public string Name { get; set; }
    }
}
