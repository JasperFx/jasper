using System;
using Jasper.Messaging;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class can_build_custom_instances : IntegrationContext
    {
        [Theory]
        [InlineData(typeof(ICommandBus))]
        [InlineData(typeof(IMessageContext))]
        [InlineData(typeof(IMessagePublisher))]
        public void can_build(Type serviceType)
        {
            Host.Get(serviceType)
                .ShouldNotBeNull();
        }

        public can_build_custom_instances(DefaultApp @default) : base(@default)
        {
        }
    }
}
