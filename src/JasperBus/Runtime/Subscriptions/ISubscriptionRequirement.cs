using System;
using System.Collections.Generic;
using JasperBus.Configuration;

namespace JasperBus.Runtime.Subscriptions
{
    public interface ISubscriptionRequirements
    {
        IEnumerable<Subscription> DetermineRequirements();
    }

    public interface ISubscriptionRequirement
    {
        IEnumerable<Subscription> Determine(ChannelGraph graph);
        void AddType(Type type);
    }
}
