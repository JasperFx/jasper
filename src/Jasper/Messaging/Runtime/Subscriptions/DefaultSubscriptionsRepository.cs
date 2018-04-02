using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Util;

namespace Jasper.Messaging.Runtime.Subscriptions
{
    public class DefaultSubscriptionsRepository : ISubscriptionsRepository
    {
        private readonly List<ServiceCapabilities> _subscriptions = new List<ServiceCapabilities>();

        public DefaultSubscriptionsRepository(SubscriptionSettings settings)
        {
            if (settings.Subscriptions.Any())
            {
                _subscriptions.Add(new ServiceCapabilities
                {
                    Subscriptions = settings.Subscriptions
                });
            }
        }

        public Task RemoveCapabilities(string serviceName)
        {
            _subscriptions.RemoveAll(x => x.ServiceName == serviceName);
            return Task.CompletedTask;
        }

        public Task<ServiceCapabilities[]> AllCapabilities()
        {
            return Task.FromResult(_subscriptions.ToArray());
        }


        public Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            var matching = _subscriptions.SelectMany(x => x.Subscriptions).Where(x => x.MessageType == messageType.ToMessageAlias()).ToArray();
            return Task.FromResult(matching);
        }

        public Task<Subscription[]> GetSubscriptions()
        {
            return Task.FromResult(_subscriptions.SelectMany(x => x.Subscriptions).ToArray());
        }

        public void Dispose()
        {
        }

        public Task PersistCapabilities(ServiceCapabilities capabilities)
        {
            _subscriptions.RemoveAll(x => x.ServiceName == capabilities.ServiceName);
            _subscriptions.Add(capabilities);
            return Task.CompletedTask;
        }

        public Task<ServiceCapabilities> CapabilitiesFor(string serviceName)
        {
            var match = _subscriptions.FirstOrDefault(x => x.ServiceName == serviceName);
            return Task.FromResult(match);
        }
    }
}
