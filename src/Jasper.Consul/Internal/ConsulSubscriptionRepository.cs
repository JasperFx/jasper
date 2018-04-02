using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Consul;
using Jasper.Messaging;
using Jasper.Messaging.Runtime.Subscriptions;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Util;
using Microsoft.CodeAnalysis.CSharp;

namespace Jasper.Consul.Internal
{
    public class ConsulSubscriptionRepository : ConsulService, ISubscriptionsRepository
    {
        private readonly ConsulSettings _settings;
        public const string SUBSCRIPTION_PREFIX = GLOBAL_PREFIX + "subscription/";
        public const string CAPABILITY_PREFEX = GLOBAL_PREFIX + "service/";

        public ConsulSubscriptionRepository(ConsulSettings settings, IChannelGraph channels, MessagingSettings envSettings)
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

        public Task RemoveCapabilities(string serviceName)
        {
            return client.KV.Delete(serviceName.ServiceCapabilityConsulId());
        }

        public Task PersistCapabilities(ServiceCapabilities capabilities)
        {
            var ops = capabilities.Subscriptions
                .Select(s => new KVTxnOp(s.ConsulId(), KVTxnVerb.Set) {Value = serialize(s)})
                .ToList();

            var persist = new KVTxnOp(capabilities.ConsulId(), KVTxnVerb.Set){Value = serialize(capabilities)};
            ops.Add(persist);

            return client.KV.Txn(ops);
        }

        public async Task<ServiceCapabilities> CapabilitiesFor(string serviceName)
        {
            var prefix = serviceName.ServiceCapabilityConsulId();
            var pair = await client.KV.Get(prefix);
            if (pair.Response == null) return null;

            return deserialize<ServiceCapabilities>(pair.Response.Value);
        }

        public async Task<ServiceCapabilities[]> AllCapabilities()
        {
            var subs = await client.KV.List(CAPABILITY_PREFEX);

            return subs.Response?.Select(kv => deserialize<ServiceCapabilities>(kv.Value)).ToArray() ?? new ServiceCapabilities[0];
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
            return $"{ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX}{subscription.GetId()}";
        }

        public static string ServiceCapabilityConsulId(this string serviceName)
        {
            return $"{ConsulSubscriptionRepository.CAPABILITY_PREFEX}{serviceName}";
        }

        public static string ConsulId(this ServiceCapabilities capabilities)
        {
            return $"{ConsulSubscriptionRepository.CAPABILITY_PREFEX}{capabilities.ServiceName}";
        }

        public static string ConsulIdPrefix(this string messageType)
        {
            return $"{ConsulSubscriptionRepository.SUBSCRIPTION_PREFIX}{messageType}";
        }
    }

}
