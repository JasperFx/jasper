using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionRequirements : ISubscriptionRequirements
    {
        private readonly SerializationGraph _serialization;
        private readonly ChannelGraph _graph;
        private readonly IList<ISubscriptionRequirement> _requirements;

        public SubscriptionRequirements(SerializationGraph serialization, ChannelGraph channels, IList<ISubscriptionRequirement> requirements)
        {
            _serialization = serialization;
            _graph = channels;
            _requirements = requirements;
        }

        public IEnumerable<Subscription> DetermineRequirements()
        {
            return _requirements.SelectMany(x => x.Determine(_graph, _serialization));
        }
    }
}
