using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Marten;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Marten;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using Weasel.Postgresql;

namespace Jasper.Persistence.Testing.Marten.Persistence.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : PostgresqlContext, IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState>
    {
        private readonly IHost _host;

        protected SagaTestHarness()
        {
            _host = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.DatabaseSchemaName = "sagas";
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                });

                _.Extensions.Include<MessageTrackingExtension>();



                _.Endpoints.PublishAllMessages().Locally();

                configure(_);
            });


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

        protected virtual void configure(JasperOptions options)
        {
            // nothing
        }

        protected Task send<T>(T message)
        {
            return _host.ExecuteAndWait(x => x.Send(message), 10000);
        }

        protected Task send<T>(T message, object sagaId)
        {
            return _host.ExecuteAndWait(x =>
            {
                var envelope = new Envelope(message)
                {
                    SagaId = sagaId.ToString()
                };

                return x.SendEnvelope(envelope);
            }, 10000);
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
