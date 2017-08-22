using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Runtime.Routing;

namespace Jasper.Bus.Runtime.Subscriptions.New
{
    public class MessagingGraph
    {
        private readonly List<MessageRoute> _routes = new List<MessageRoute>();
        private readonly List<PublishedMessage> _noSubscribers = new List<PublishedMessage>();
        private readonly List<Subscription> _noPublishers = new List<Subscription>();
        private readonly List<PublisherSubscriberMismatch> _mismatches = new List<PublisherSubscriberMismatch>();

        public MessagingGraph(ServiceCapabilities[] capabilities)
        {
            Services = organizeServices(capabilities);


            var published = organizePublishing(capabilities);

            var subscriptions = organizeSubscriptions(capabilities);

            _noSubscribers.AddRange(published.Where(x => !subscriptions.ContainsKey(x.Key)).SelectMany(x => x.Value));
            _noPublishers.AddRange(subscriptions.Where(x => !published.ContainsKey(x.Key)).SelectMany(x => x.Value));

            var matches = published.Keys.Intersect(subscriptions.Keys).ToArray();
            foreach (var messageType in matches)
            {
                var senders = published[messageType];
                var receivers = subscriptions[messageType];

                foreach (var sender in senders)
                {
                    foreach (var receiver in receivers)
                    {
                        tryToMatch(sender, receiver);


                    }
                }
            }
        }

        public Dictionary<string, ServiceCapabilities> Services { get; }

        private static Dictionary<string, ServiceCapabilities> organizeServices(ServiceCapabilities[] capabilities)
        {
            var services = new Dictionary<string, ServiceCapabilities>();
            foreach (var capability in capabilities)
            {
                services.SmartAdd(capability.ServiceName, capability);
            }

            return services;
        }

        private void tryToMatch(PublishedMessage sender, Subscription receiver)
        {
            if (MessageRoute.TryToRoute(sender, receiver, out MessageRoute route,
                out PublisherSubscriberMismatch mismatch))
            {
                _routes.Add(route);
            }
            else
            {
                _mismatches.Add(mismatch);
            }

        }


        private static Dictionary<string, List<Subscription>> organizeSubscriptions(ServiceCapabilities[] capabilities)
        {
            var groups = capabilities.SelectMany(x => x.Subscriptions).GroupBy(x => x.MessageType);
            var subscriptions = new Dictionary<string, List<Subscription>>();
            foreach (var @group in groups)
            {
                subscriptions.Add(@group.Key, new List<Subscription>(@group));
            }

            return subscriptions;
        }

        private static Dictionary<string, List<PublishedMessage>> organizePublishing(ServiceCapabilities[] capabilities)
        {
            var groups = capabilities.SelectMany(x => x.Published).GroupBy(x => x.MessageType);
            var published = new Dictionary<string, List<PublishedMessage>>();
            foreach (var @group in groups)
            {
                published.Add(@group.Key, new List<PublishedMessage>(@group));
            }
            return published;
        }

        public MessageRoute[] Matched => _routes.ToArray();

        public PublishedMessage[] NoSubscribers => _noSubscribers.ToArray();

        public Subscription[] NoPublishers => _noPublishers.ToArray();

        public PublisherSubscriberMismatch[] Mismatches => _mismatches.ToArray();

    }
}
