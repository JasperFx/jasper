using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.LightningQueues;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionsHandler
    {
        private readonly ChannelGraph _graph;
        private readonly ISubscriptionsStorage _subscriptions;
        private readonly INodeDiscovery _nodeDiscovery;

        public SubscriptionsHandler(
            ChannelGraph graph,
            ISubscriptionsStorage subscriptions,
            INodeDiscovery nodeDiscovery)
        {
            _graph = graph;
            _subscriptions = subscriptions;
            _nodeDiscovery = nodeDiscovery;
        }

        public IEnumerable<object> Handle(SubscriptionRequested message)
        {
            var modifiedSubscriptions = message.Subscriptions
                .Select(x =>
                {
                    x.Id = Guid.NewGuid();
                    x.NodeName = _graph.Name;
                    x.Role = SubscriptionRole.Publishes;
                    x.Source = x.Source.ToMachineUri();
                    return x;
                });

            _subscriptions.PersistSubscriptions(modifiedSubscriptions);

            var result = UpdateNodes();
            return result;
        }

        public void Handle(SubscriptionsChanged message)
        {
            _subscriptions.LoadSubscriptions(SubscriptionRole.Publishes);
        }

        private IEnumerable<object> UpdateNodes()
        {
            return _nodeDiscovery.FindPeers().Select(node => new CascadeEnvelope
            {
                Message = new SubscriptionsChanged(),
                Destination = node.Address
            });
        }
    }

    public class CascadeEnvelope : ISendMyself
    {
        public Uri Destination { get; set; }
        public object Message { get; set; }

        public Envelope CreateEnvelope(Envelope original)
        {
            var env = original.ForResponse(Message);
            env.Destination = Destination;
            return env;
        }
    }

    public class SubscriptionRequested
    {
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();

        public Subscription[] Subscriptions
        {
            get { return _subscriptions.ToArray(); }
            set
            {
                _subscriptions.Clear();
                if (value != null) _subscriptions.AddRange(value);
            }
        }
    }

    public class SubscriptionsChanged
    {
    }
}
