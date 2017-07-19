using System;
using System.Collections.Generic;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionRequirements
    {
        IEnumerable<Subscription> DetermineRequirements();
    }

    public interface ISubscriptionRequirement
    {
        IEnumerable<Subscription> Determine(ChannelGraph channels, SerializationGraph serialization);
        void AddType(Type type);
    }
}
