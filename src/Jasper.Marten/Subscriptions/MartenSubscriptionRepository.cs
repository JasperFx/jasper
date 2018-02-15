using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime.Subscriptions;
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

        public async Task RemoveCapabilities(string serviceName)
        {
            using (var session = _documentStore.LightweightSession())
            {
                session.Delete<ServiceCapabilities>(serviceName);
                await session.SaveChangesAsync();
            }
        }

        public async Task PersistCapabilities(ServiceCapabilities capabilities)
        {
            using (var session = _documentStore.LightweightSession())
            {
                session.Store(capabilities);
                await session.SaveChangesAsync();
            }
        }

        public async Task<ServiceCapabilities> CapabilitiesFor(string serviceName)
        {
            using (var session = _documentStore.QuerySession())
            {
                return await session.LoadAsync<ServiceCapabilities>(serviceName);
            }
        }

        public async Task<ServiceCapabilities[]> AllCapabilities()
        {
            using (var session = _documentStore.QuerySession())
            {
                return (await session.Query<ServiceCapabilities>().ToListAsync()).ToArray();
            }
        }

        public async Task<Subscription[]> GetSubscribersFor(Type messageType)
        {
            using (var query = _documentStore.QuerySession())
            {
                var docs = await query.Query<ServiceCapabilities>().SelectMany(x => x.Subscriptions)
                    .Where(x => x.MessageType == messageType.ToMessageAlias())
                    .ToListAsync();

                return docs.ToArray();
            }
        }

        public async Task<Subscription[]> GetSubscriptions()
        {
            using (var query = _documentStore.QuerySession())
            {
                var docs = await query.Query<ServiceCapabilities>().SelectMany(x => x.Subscriptions)
                    .ToListAsync();

                return docs.ToArray();
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
