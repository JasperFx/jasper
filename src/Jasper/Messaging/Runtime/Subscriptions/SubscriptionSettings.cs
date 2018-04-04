using System;
using Newtonsoft.Json;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public class SubscriptionSettings
    {
        [JsonProperty("subscriptions")]
        public Subscription[] Subscriptions { get; set; } = new Subscription[0];
    }
}
