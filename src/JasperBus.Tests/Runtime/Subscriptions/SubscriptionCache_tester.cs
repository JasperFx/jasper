using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperBus.Runtime;
using JasperBus.Runtime.Subscriptions;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Subscriptions
{
    public class SubscriptionCache_tester
    {
        private readonly SubscriptionsCache _cache;

        public SubscriptionCache_tester()
        {
            _cache = new SubscriptionsCache();
        }

        [Fact]
        public void can_add_subscriptions()
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
        public static Subscription NewSubscription(string nodeName = null)
        {
            return new Subscription
            {
                MessageType = Guid.NewGuid().ToString(),
                NodeName = nodeName ?? "TheNode",
                Receiver = "memory://receiver".ToUri(),
                Source = "memory://source".ToUri(),
                Role = SubscriptionRole.Subscribes
            };
        }

        public static Subscription ExistingSubscription(string nodeName = null)
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
