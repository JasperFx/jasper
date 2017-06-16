using System;
using System.Collections.Generic;
using Jasper.Bus.Configuration;

namespace Jasper.Bus.Runtime.Subscriptions
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
