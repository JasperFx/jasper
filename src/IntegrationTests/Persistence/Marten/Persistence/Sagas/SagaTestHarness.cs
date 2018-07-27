using System;
using System.Threading.Tasks;
using Jasper;
using Jasper.Messaging.Model;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Sagas;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Marten;
using Marten;
using Servers;

namespace IntegrationTests.Persistence.Marten.Persistence.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : MartenContext, IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState>
    {
        private readonly MessageHistory _history;
        private readonly JasperRuntime _runtime;

        protected SagaTestHarness(DockerFixture<MartenContainer> fixture) : base(fixture)
        {
            _runtime = JasperRuntime.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();
                _.MartenConnectionStringIs(MartenContainer.ConnectionString);
                _.Include<MartenBackedPersistence>();

                _.Include<MessageTrackingExtension>();

                _.Settings.ConfigureMarten(x =>
                    {
                        x.DatabaseSchemaName = "sagas";
                        x.AutoCreateSchemaObjects = AutoCreate.All;
                    }
                );

                _.Publish.AllMessagesTo(TransportConstants.LoopbackUri);

                configure(_);
            });

            _history = _runtime.Get<MessageHistory>();

            var store = _runtime.Get<IDocumentStore>();
            store.Advanced.Clean.CompletelyRemoveAll();
            store.Tenancy.Default.EnsureStorageExists(typeof(Envelope));
        }

        public void Dispose()
        {
            _runtime?.Dispose();
        }

        protected string codeFor<T>()
        {
            return _runtime.Get<HandlerGraph>().HandlerFor<T>().Chain.SourceCode;
        }

        protected virtual void configure(JasperRegistry registry)
        {
            // nothing
        }

        protected Task send<T>(T message)
        {
            return _history.WatchAsync(() => _runtime.Messaging.Send(message), 10000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _history.WatchAsync(() => _runtime.Messaging.Send(message, e => e.SagaId = sagaId.ToString()),
                10000);
        }

        protected async Task<TSagaState> LoadState(Guid id)
        {
            using (var session = _runtime.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(int id)
        {
            using (var session = _runtime.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(long id)
        {
            using (var session = _runtime.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(string id)
        {
            using (var session = _runtime.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }
    }
}
