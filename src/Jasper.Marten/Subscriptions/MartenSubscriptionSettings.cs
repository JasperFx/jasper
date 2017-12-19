using System;
using Jasper.Bus.Runtime.Subscriptions;
using Marten;

namespace Jasper.Marten.Subscriptions
{
    public class MartenSubscriptionSettings : IDisposable
    {
        private readonly Lazy<IDocumentStore> _store;

        public MartenSubscriptionSettings()
        {
            StoreOptions.Schema.For<ServiceCapabilities>().Identity(x => x.ServiceName);

            _store = new Lazy<IDocumentStore>(() => new DocumentStore(StoreOptions));

            var connectionString = Environment.GetEnvironmentVariable("marten_subscription_database");
            if (connectionString != null)
            {
                StoreOptions.Connection(connectionString);
            }
        }

        public StoreOptions StoreOptions { get; } = new StoreOptions();

        internal IDocumentStore Store => _store.Value;

        public void Dispose()
        {
            if (_store.IsValueCreated)
            {
                _store.Value.Dispose();
            }
        }
    }
}
