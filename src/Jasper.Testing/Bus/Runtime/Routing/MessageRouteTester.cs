using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Routing;
using Jasper.Bus.Runtime.Subscriptions;
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

            var subscription = new Subscription(typeof(Message1), "loopback://one".ToUri());
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

            var subscription = new Subscription(typeof(Message1), "fake://one".ToUri());
            subscription.Accept = new string[]{"three"};

            MessageRoute.TryToRoute(published, subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
                .ShouldBeFalse();

            mismatch.IncompatibleContentTypes.ShouldBeTrue();
            mismatch.IncompatibleTransports.ShouldBeTrue();
        }

        [Fact]
        public void pick_the_first_matching_content_type_that_is_not_app_json()
        {
            var published = new PublishedMessage(typeof(Message1))
            {
                ContentTypes = new string[] {"application/json", "app/v2", "app/v3"},
                Transports = new string[]{"loopback"}

            };

            var subscription = new Subscription(typeof(Message1), "loopback://one".ToUri());
            subscription.Accept = new string[]{"application/json", "app/v1", "app/v3"};

            MessageRoute.TryToRoute(published, subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
                .ShouldBeTrue();

            route.ContentType.ShouldBe("app/v3");
        }

        [Fact]
        public void use_json_if_that_is_the_only_match    ()
        {
            var published = new PublishedMessage(typeof(Message1))
            {
                ContentTypes = new string[] {"application/json", "app/v2", "app/v3"},
                Transports = new string[]{"loopback"}

            };

            var subscription = new Subscription(typeof(Message1), "loopback://one".ToUri());
            subscription.Accept = new string[]{"application/json", "app/v4", "app/v5"};

            MessageRoute.TryToRoute(published, subscription, out MessageRoute route, out PublisherSubscriberMismatch mismatch)
                .ShouldBeTrue();

            route.ContentType.ShouldBe("application/json");
        }
    }
}
