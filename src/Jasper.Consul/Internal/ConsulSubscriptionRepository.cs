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
            var ops = subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Set) {Value = serialize(s)})
                .ToList();

            return client.KV.Txn(ops);
        }

        public async Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            var subscriptions = await AllSubscriptions();
            return subscriptions.Where(s => s.NodeName == ServiceName && s.Role == subscriptionRole).ToArray();
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            var ops = subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Delete));

            return client.KV.Txn(ops
                .ToList()
            );
        }

        public async Task<IEnumerable<Subscription>> AllSubscriptions()
        {
            var subs = await client.KV.List(SUBSCRIPTION_PREFIX);
            return subs.Response?.Select(kv => deserialize<Subscription>(kv.Value)) ?? new Subscription[0];
        }






    }

    public static class SubscriptionExtensions
    {
        public static string ConsulId(this Subscription subscription)
        {
            if (subscription.Id == Guid.Empty) subscription.Id = Guid.NewGuid();

            return $"{ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX}{subscription.Id}";
        }
    }

}
