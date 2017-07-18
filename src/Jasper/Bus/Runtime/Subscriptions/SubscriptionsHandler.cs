using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Routing;
using Jasper.Util;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionsHandler
    {
        private readonly ChannelGraph _graph;
        private readonly ISubscriptionsStorage _subscriptions;
        private readonly INodeDiscovery _nodeDiscovery;
        private readonly IMessageRouter _router;

        public SubscriptionsHandler(ChannelGraph graph, ISubscriptionsStorage subscriptions, INodeDiscovery nodeDiscovery, IMessageRouter router)
        {
            _graph = graph;
            _subscriptions = subscriptions;
            _nodeDiscovery = nodeDiscovery;
            _router = router;
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

            _router.ClearAll();

            return UpdateNodes();
        }

        public void Handle(SubscriptionsChanged message)
        {
            _subscriptions.LoadSubscriptions(SubscriptionRole.Publishes);
            _router.ClearAll();
        }

        private IEnumerable<object> UpdateNodes()
        {
            return _nodeDiscovery.FindPeers().Select(node => new CascadeEnvelope
            {
                Message = new SubscriptionsChanged(),
                Destination = node.ControlChannel
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
