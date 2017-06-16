using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public class SubscriptionCacheTester
    {
        private readonly SubscriptionsCache _cache;

        public SubscriptionCacheTester()
        {
            _cache = new SubscriptionsCache();
        }

        [Fact]
        public void add_subscriptions()
        {
            var subscriptions1 = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            var subscriptions2 = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            _cache.Store(subscriptions1);
            _cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions1);

            _cache.Store(subscriptions2);
            _cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions1.Concat(subscriptions2));
        }

        [Fact]
        public void clears_all_subscriptions()
        {
            var subscriptions = new[]
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            _cache.Store(subscriptions);
            _cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions);

            _cache.ClearAll();
            _cache.ActiveSubscriptions.ShouldHaveCount(0);
        }

        [Fact]
        public void removes_subscription()
        {
            var subscriptions = new List<Subscription>
            {
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription(),
                Subs.ExistingSubscription()
            };

            _cache.Store(subscriptions);
            _cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions);

            _cache.Remove(subscriptions[2]);
            subscriptions.RemoveAt(2);
            _cache.ActiveSubscriptions.ShouldHaveCount(3);
            _cache.ActiveSubscriptions.ShouldHaveTheSameElementsAs(subscriptions);
        }

    }

    public static class Subs
    {
        public static Subscription NewSubscription(string nodeName = null, SubscriptionRole role = SubscriptionRole.Subscribes)
        {
            return new Subscription
            {
                MessageType = Guid.NewGuid().ToString(),
                NodeName = nodeName ?? "TheNode",
                Receiver = "memory://receiver".ToUri(),
                Source = "memory://source".ToUri(),
                Role = role
            };
        }

        public static Subscription ExistingSubscription(string nodeName = null, SubscriptionRole role = SubscriptionRole.Subscribes)
        {
            var subscription = NewSubscription();
            subscription.Id = Guid.NewGuid();

            if (nodeName.IsNotEmpty())
            {
                subscription.NodeName = nodeName;
            }

            return subscription;
        }
    }
}
