using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions.New;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Routing
{
    public class MessageRouteTester
    {
        [Fact]
        public void mismatch_with_no_matching_content_types()
        {
            var published = new PublishedMessage(typeof(Message1))
            {
                ContentTypes = new string[] {"one", "two"}
            };

            var subscription = new NewSubscription(typeof(Message1), "loopback://one".ToUri());
            subscription.Accept = new string[]{"three"};

            MessageRoute.TryToRoute(published, subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
                .ShouldBeFalse();

            mismatch.IncompatibleContentTypes.ShouldBeTrue();
            mismatch.PublishedContentTypes.ShouldBe(published.ContentTypes);
            mismatch.SubscriberContentTypes.ShouldBe(subscription.Accept);
        }

        [Fact]
        public void mismatch_with_no_matching_content_types_and_transport()
        {
            var published = new PublishedMessage(typeof(Message1))
            {
                ContentTypes = new string[] {"one", "two"},
                Transports = new string[]{"jasper"}
            };

            var subscription = new NewSubscription(typeof(Message1), "fake://one".ToUri());
            subscription.Accept = new string[]{"three"};

            MessageRoute.TryToRoute(published, subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
                .ShouldBeFalse();

            mismatch.IncompatibleContentTypes.ShouldBeTrue();
            mismatch.IncompatibleTransports.ShouldBeTrue();
        }
    }
}
