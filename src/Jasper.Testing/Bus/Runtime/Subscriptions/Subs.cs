using System;
using Baseline;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Testing.Bus.Runtime.Subscriptions
{
    public static class Subs
    {
        public static Subscription NewSubscription(string nodeName = null, SubscriptionRole role = SubscriptionRole.Subscribes)
        {
            return new Subscription
            {
                MessageType = Guid.NewGuid().ToString(),
                Publisher = nodeName ?? "TheNode",
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
                subscription.Publisher = nodeName;
            }

            return subscription;
        }
    }
}