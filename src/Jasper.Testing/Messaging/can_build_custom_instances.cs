using System;
using Jasper.Messaging;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class can_build_custom_instances
    {
        [Theory]
        [InlineData(typeof(ICommandBus))]
        [InlineData(typeof(IMessageContext))]
        [InlineData(typeof(IMessagePublisher))]
        public void can_build(Type serviceType)
        {
            using (var runtime = JasperHost.Basic())
            {
                runtime.Container.GetInstance(serviceType)
                    .ShouldNotBeNull();
            }
        }
    }
}
