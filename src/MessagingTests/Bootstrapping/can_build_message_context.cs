using Jasper.Messaging;
using Shouldly;
using Xunit;

namespace MessagingTests.Bootstrapping
{
    public class can_build_message_context : IntegrationContext
    {
        public can_build_message_context(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void can_build_i_message_context()
        {
            Host.Get<IMessageContext>().ShouldNotBeNull();
            
            Host.Get<ThingThatUsesContext>()
                .Context.ShouldNotBeNull();
        }

        public class ThingThatUsesContext
        {
            public IMessageContext Context { get; }

            public ThingThatUsesContext(IMessageContext context)
            {
                Context = context;
            }
        }
    }
}
