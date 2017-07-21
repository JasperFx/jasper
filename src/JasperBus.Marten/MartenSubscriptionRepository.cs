using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Util;
using Marten;
using Marten.Schema.Arguments;

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

        public Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                var existing = session.Query<Subscription>().Where(x => x.NodeName == _graph.Name).ToList();
                var newReqs = subscriptions.Where(x => !existing.Contains(x)).ToList();
                session.Store(newReqs);
                return session.SaveChangesAsync();
            }
        }

        public async Task<Subscription[]> LoadSubscriptions(SubscriptionRole subscriptionRole)
        {
            using (var session = _documentStore.LightweightSession())
            {
                return (await session.Query<Subscription>()
                    .Where(x => x.NodeName == _graph.Name && x.Role == subscriptionRole).ToListAsync()).ToArray();
            }
        }

        public Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                foreach (var subscription in subscriptions)
                {
                    session.Delete(subscription.Id);
                }

                return session.SaveChangesAsync();
            }
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            using (var query = _documentStore.QuerySession())
            {
                var docs = await query.Query<Subscription>()
                    .Where(x => x.MessageType == messageType.ToTypeAlias() && x.NodeName == _graph.Name &&
                                x.Role == SubscriptionRole.Publishes)
                    .ToListAsync();

                return docs.ToArray();
            }
        }

        public void Dispose()
        {
            _documentStore?.Dispose();
        }
    }
}
