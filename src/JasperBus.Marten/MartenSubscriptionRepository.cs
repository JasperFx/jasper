using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Subscriptions;
using Marten;

namespace JasperBus.Marten
{
    public class MartenSubscriptionRepository : ISubscriptionsRepository
    {
        private readonly ChannelGraph _graph;
        private readonly IDocumentStore _documentStore;

        public MartenSubscriptionRepository(IDocumentStore documentStore, ChannelGraph graph)
        {
            _graph = graph;
            _documentStore = documentStore;
        }

        public void PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                var existing = session.Query<Subscription>().Where(x => x.NodeName == _graph.Name).ToList();
                var newReqs = subscriptions.Where(x => !existing.Contains(x)).ToList();
                session.Store(newReqs);
                session.SaveChanges();
            }
        }

        public IEnumerable<Subscription> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            using (var session = _documentStore.LightweightSession())
            {
                return session.Query<Subscription>()
                    .Where(x => x.NodeName == _graph.Name && x.Role == subscriptionRole);
            }
        }

        public void RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                subscriptions.Each(sub => session.Delete<Subscription>(sub.Id));
                session.SaveChanges();
            }
        }

        public void Dispose()
        {
            _documentStore?.Dispose();
        }
    }
}
