using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionActivator
    {
        Task Activate();
    }

    public class SubscriptionActivator : ISubscriptionActivator
    {
        private readonly ISubscriptionsStorage _subscriptions;
        private readonly IEnvelopeSender _sender;
        private readonly INodeDiscovery _nodeDiscovery;
        private readonly IEnumerable<ISubscriptionRequirements> _requirements;
        private readonly ChannelGraph _channels;

        public SubscriptionActivator(
            ISubscriptionsStorage subscriptions,
            IEnvelopeSender sender,
            INodeDiscovery nodeDiscovery,
            IEnumerable<ISubscriptionRequirements> requirements,
            ChannelGraph channels)
        {
            _subscriptions = subscriptions;
            _sender = sender;
            _nodeDiscovery = nodeDiscovery;
            _requirements = requirements;
            _channels = channels;
        }

        public async Task Activate()
        {
            await _nodeDiscovery.Register(_channels);
            await setupSubscriptions(_channels);
        }

        private async Task setupSubscriptions(ChannelGraph graph)
        {
            var staticSubscriptions = _requirements
                .SelectMany(x => x.DetermineRequirements())
                .Select(x =>
                {
                    x.Id = Guid.NewGuid();
                    x.NodeName = graph.Name;
                    x.Role = SubscriptionRole.Subscribes;
                    x.Receiver = x.Receiver.ToMachineUri();
                    return x;
                }).ToArray();

            await _subscriptions.PersistSubscriptions(staticSubscriptions);

            await sendSubscriptions();
        }

        private async Task sendSubscriptions()
        {
            var subscriptions = await _subscriptions.LoadSubscriptions(SubscriptionRole.Subscribes);

                subscriptions.GroupBy(x => x.Source)
                .Each(group =>
                {
                    var envelope = new Envelope
                    {
                        Message = new SubscriptionRequested
                        {
                            Subscriptions = group.Each(x => x.Source = x.Source.ToMachineUri()).ToArray()
                        },
                        Destination = group.Key
                    };

                    _sender.Send(envelope);
                });
        }
    }
}
