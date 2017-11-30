using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Util;
using Marten;

namespace Jasper.Marten.Subscriptions
{
    public class MartenSubscriptionRepository : ISubscriptionsRepository
    {
        private readonly IDocumentStore _documentStore;

        public MartenSubscriptionRepository(MartenSubscriptionSettings settings)
        {
            _documentStore = settings.Store;
        }

        public async Task PersistSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(subscriptions.ToArray());
                await session.SaveChangesAsync();
            }
        }

        public async Task RemoveSubscriptions(IEnumerable<Subscription> subscriptions)
        {
            using (var session = _documentStore.LightweightSession())
            {
                foreach (var subscription in subscriptions)
                    session.Delete(subscription.Id);

                await session.SaveChangesAsync();
            }
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            using (var query = _documentStore.QuerySession())
            {
                var docs = await query.Query<Subscription>()
                    .Where(x => x.MessageType == messageType.ToMessageAlias())
                    .ToListAsync();

                return docs.ToArray();
            }
        }

        public async Task<Subscription[]> GetSubscriptions()
        {
            using (var query = _documentStore.QuerySession())
            {
                var docs = await query.Query<Subscription>()
                    .ToListAsync();

                return docs.ToArray();
            }
        }

        public async Task ReplaceSubscriptions(string serviceName, Subscription[] subscriptions)
        {
            using (var session = _documentStore.OpenSession())
            {
                session.DeleteWhere<Subscription>(x => x.ServiceName == serviceName);
                session.Store(subscriptions);

                await session.SaveChangesAsync();
            }
        }

        public void Dispose()
        {
            _documentStore?.Dispose();
        }

        public async Task<IReadOnlyList<Subscription>> AllSubscriptions()
        {
            using (var query = _documentStore.QuerySession())
            {
                var list = await query.Query<Subscription>().ToListAsync();
                return list;
            }
        }
    }
}
