using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Jasper.Bus.Runtime.Subscriptions
{
    public interface ISubscriptionsCache
    {
        void ClearAll();

        void Store(IEnumerable<Subscription> subscriptions);

        void Remove(Subscription subscription);

        Subscription[] ActiveSubscriptions { get; }
    }

    // What if we said that the control channel has no parallelism?
    public class SubscriptionsCache : ISubscriptionsCache
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();

        public void ClearAll()
        {
            _subscriptions.Clear();
        }

        public void Store(IEnumerable<Subscription> subscriptions)
        {
            subscriptions.Where(x => !_subscriptions.Contains(x)).Each(x => _subscriptions.Add(x));
        }

        public void Remove(Subscription subscription)
        {
            _subscriptions.Remove(subscription);
        }

        public Subscription[] ActiveSubscriptions => _subscriptions.ToArray();
    }
}
