using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Transports.Configuration;
using Jasper.Util;
using Microsoft.CodeAnalysis.CSharp;

namespace Jasper.Consul.Internal
{
    public class ConsulSubscriptionRepository : ConsulService, ISubscriptionsRepository
    {
        private readonly ConsulSettings _settings;
        public const string SUBSCRIPTION_PREFIX = GLOBAL_PREFIX + "subscription/";

        public ConsulSubscriptionRepository(ConsulSettings settings, IChannelGraph channels, BusSettings envSettings)
            : base(settings, channels, envSettings)
        {
            _settings = settings;
        }

        public void Dispose()
        {
            // Nothing, ConsulClient is disposed elsewhere
        }

        public override string ToString()
        {
            return $"Consul-backed subscriptions";
        }

        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            var ops = subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Set) {Value = serialize(s)})
                .ToList();

            return client.KV.Txn(ops);
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {


            var ops = subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Delete));

            return client.KV.Txn(ops
                .ToList()
            );
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            var prefix = messageType.ToMessageAlias().ConsulIdPrefix();
            var subs = await client.KV.List(prefix);
            return subs.Response?.Select(kv => deserialize<Subscription>(kv.Value)).ToArray() ?? new Subscription[0];
        }

        public async Task<Subscription[]> GetSubscriptions()
        {
            var subs = await AllSubscriptions();
            return subs.ToArray();
        }

        public async Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions)
        {
            var existing = await AllSubscriptions();

            var toRemove = existing.Where(x => x.ServiceName == serviceName).Select(x => new KVTxnOp(x.ConsulId(), KVTxnVerb.Delete));

            var adds = subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Set) {Value = serialize(s)});

            await client.KV.Txn(toRemove.Concat(adds).ToList());
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
            return $"{ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX}{subscription.Id}";
        }

        public static string ConsulIdPrefix(this string messageType)
        {
            return $"{ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX}{messageType}";
        }
    }

}
