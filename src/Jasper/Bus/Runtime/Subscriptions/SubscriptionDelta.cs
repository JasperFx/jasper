using System.Linq;
using Jasper.Bus.Runtime.Subscriptions.New;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public class SubscriptionDelta
    {
        public SubscriptionDelta(Subscription[] expected, Subscription[] actual)
        {
            Matched = expected.Intersect(actual).ToArray();
            NewSubscriptions = expected.Where(x => !Matched.Contains(x)).ToArray();
            ObsoleteSubscriptions = actual.Where(x => !Matched.Contains(x)).ToArray();
        }

        public Subscription[] ObsoleteSubscriptions { get; set; }

        public Subscription[] NewSubscriptions { get; }

        public Subscription[] Matched { get; }
    }
}
