using System.Collections.Generic;
using System.Linq;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public class SubscriptionRequirements : ISubscriptionRequirements
    {
        private readonly ChannelGraph _graph;
        private readonly IList<ISubscriptionRequirement> _requirements;

        public SubscriptionRequirements(ChannelGraph graph, IList<ISubscriptionRequirement> requirements)
        {
            _graph = graph;
            _requirements = requirements;
        }

        public IEnumerable<Subscription> DetermineRequirements()
        {
            return _requirements.SelectMany(x => x.Determine(_graph));
        }
    }
}
