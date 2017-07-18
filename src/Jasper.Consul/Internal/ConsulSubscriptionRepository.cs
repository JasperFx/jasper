using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            return client.KV.Txn(
                subscriptions
                    .Select(s =>
                    {
                        if (s.Id == Guid.Empty) s.Id = Guid.NewGuid();
                        return new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id.ToString(), KVTxnVerb.Set) {Value = serialize(s)};
                    })
                    .ToList()
            );
        }

        public async Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            var subscriptions = await AllSubscriptions();
            return subscriptions.Where(s => s.NodeName == ServiceName && s.Role == subscriptionRole).ToArray();
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            return client.KV.Txn(subscriptions
                .Select(s => new KVTxnOp(SUBSCRIPTION_PREFIX + s.Id, KVTxnVerb.Delete))
                .ToList()
            );
        }

        public async Task<IEnumerable<Subscription>> AllSubscriptions()
        {
            var subs = await client.KV.List(SUBSCRIPTION_PREFIX);
            return subs.Response?.Select(kv => deserialize<Subscription>(kv.Value)) ?? new Subscription[0];
        }






    }

}
