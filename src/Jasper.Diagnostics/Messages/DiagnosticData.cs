using System.Collections.Generic;
using System.Linq;
using Jasper.Bus.Runtime.Subscriptions;

namespace Jasper.Diagnostics.Messages
{
    public class DiagnosticData : ClientMessage
    {
        public DiagnosticData(IEnumerable<ChainModel> chains, IEnumerable<Subscription> storedSubs, PublishedMessage[] pubs, Subscription[] declaredSubs)
            : base("diagnostic-data")
        {
            Chains = chains.ToArray();
            StoredSubscriptions = storedSubs.ToArray();
            Publications = pubs;
            DeclaredSubscriptions = declaredSubs;
        }

        public Subscription[] StoredSubscriptions { get; }
        public Subscription[] DeclaredSubscriptions { get; }
        public ChainModel[] Chains { get; }
        public PublishedMessage[] Publications { get; }
    }
}
