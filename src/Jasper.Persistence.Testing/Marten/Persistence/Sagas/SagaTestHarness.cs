using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Messaging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Persistence.Testing.Marten.Persistence.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : PostgresqlContext, IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState>
    {
        private readonly MessageHistory _history;
        private readonly IHost _host;

        protected SagaTestHarness()
        {
            _host = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();
                _.MartenConnectionStringIs(Servers.PostgresConnectionString);
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

            _history = _host.Get<MessageHistory>();

            var store = _host.Get<IDocumentStore>();
            store.Advanced.Clean.CompletelyRemoveAll();

            _host.RebuildMessageStorage();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        protected string codeFor<T>()
        {
            return _host.Get<HandlerGraph>().HandlerFor<T>().Chain.SourceCode;
        }

        protected virtual void configure(JasperRegistry registry)
        {
            // nothing
        }

        protected Task send<T>(T message)
        {
            return _history.WatchAsync(() => _host.Send(message), 10000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _history.WatchAsync(() => _host.Get<IMessagePublisher>().Send(message, e => e.SagaId = sagaId.ToString()),
                10000);
        }

        protected async Task<TSagaState> LoadState(Guid id)
        {
            using (var session = _host.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(int id)
        {
            using (var session = _host.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(long id)
        {
            using (var session = _host.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }

        protected async Task<TSagaState> LoadState(string id)
        {
            using (var session = _host.Get<IQuerySession>())
            {
                return await session.LoadAsync<TSagaState>(id);
            }
        }
    }
}
