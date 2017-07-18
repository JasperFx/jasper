using System;
using System.Collections.Generic;
using System.Linq;
using Consul;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Consul.Internal
{
    public class ConsulSubscriptionRepository : ConsulService, ISubscriptionsRepository
    {
        public const string SUBSCRIPTION_PREFIX = GLOBAL_PREFIX + "subscription/";

        public ConsulSubscriptionRepository(ConsulSettings settings, ChannelGraph channels, EnvironmentSettings envSettings)
            : base(settings, channels, envSettings)
        {
        }

        public void Dispose()
        {
            // Nothing, ConsulClient is disposed elsewhere
        }

        public void PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            client.KV.Txn(
                subscriptions
                    .Select(s =>
                    {
                        if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
                        return new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id.ToString(), KVTxnVerb.Set) {Value = serialize(s)};
                    })
                    .ToList()
            ).Wait();
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            return AllSubscriptions().Where(s => s.NodeName == ServiceName && s.Role == subscriptionRole);
        }

        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            client.KV.Txn(subscriptions
                .Select(s => new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id, KVTxnVerb.Delete))
                .ToList()
            ).Wait();
        }

        public IEnumerable<Subscription> AllSubscriptions()
        {
            var subs = client.KV.List(SUBSCRIPTION_PREFIX).Result;
            return subs.Response?.Select(kv => deserialize<Subscription>(kv.Value)) ?? new Subscription[0];
        }






    }

}
