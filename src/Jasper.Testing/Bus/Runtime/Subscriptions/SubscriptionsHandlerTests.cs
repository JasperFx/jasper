using System;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions;
using NSubstitute;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionsHandlerTests : InteractionContext<SubscriptionsHandler>
    {
        [Fact]
        public void should_reset_message_router_on_subscriptions_changed()
        {
            ClassUnderTest.Handle(new SubscriptionsChanged());

            MockFor<IMessageRouter>().Received().ClearAll();
        }
        
        [Fact]
        public void should_reset_message_router_on_subscriptions_requested()
        {
            ClassUnderTest.Handle(new SubscriptionRequested {Subscriptions = Array.Empty<Subscription>()});

            MockFor<IMessageRouter>().Received().ClearAll();
        }
    }
}
