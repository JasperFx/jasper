﻿using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Transports.LightningQueues;

namespace JasperBus.Runtime.Subscriptions
{
    public interface ISubscriptionActivator
    {
        void Activate();
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

        public void Activate()
        {
            _nodeDiscovery.Register(_channels);
            setupSubscriptions(_channels);
        }

        private void setupSubscriptions(ChannelGraph graph)
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
                });

            _subscriptions.PersistSubscriptions(staticSubscriptions);

            sendSubscriptions();
        }

        private void sendSubscriptions()
        {
            _subscriptions.LoadSubscriptions(SubscriptionRole.Subscribes)
                .GroupBy(x => x.Source)
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
