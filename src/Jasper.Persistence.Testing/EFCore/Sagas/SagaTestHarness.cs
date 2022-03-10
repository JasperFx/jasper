using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper.Persistence.Database;
using Jasper.Persistence.EntityFrameworkCore;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.Testing.SqlServer;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestingSupport;
using Weasel.Core;
using Weasel.SqlServer;
using Weasel.SqlServer.Tables;
using Xunit;
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : SqlServerContext, IAsyncLifetime
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private readonly ITestOutputHelper _output;
        private readonly IHost _host;

        protected SagaTestHarness(ITestOutputHelper output)
        {
            _output = output;
            _host = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                _.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);

                _.Services.AddDbContext<SagaDbContext>(x => x.UseSqlServer(Servers.SqlServerConnectionString));

                _.Extensions.UseEntityFrameworkCorePersistence();

                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.PublishAllMessages().Locally();

                configure(_);
            });
        }

        protected override async Task initialize()
        {
            var tables = new ISchemaObject[]
            {
                new WorkflowStateTable<Guid>("GuidWorkflowState"),
                new WorkflowStateTable<int>("IntWorkflowState"),
                new WorkflowStateTable<long>("LongWorkflowState"),
                new WorkflowStateTable<string>("StringWorkflowState"),
            };

            await using var conn = new SqlConnection(Servers.SqlServerConnectionString);
            await conn.OpenAsync();

            var migration = await SchemaMigration.Determine(conn, tables);
            await new SqlServerMigrator().ApplyAll(conn, migration, AutoCreate.All);

            await _host.RebuildMessageStorage();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        internal class WorkflowStateTable<T> : Table
        {
            public WorkflowStateTable(string tableName) : base(tableName)
            {
                AddColumn<T>("Id").AsPrimaryKey();
                AddColumn<bool>("one");
                AddColumn<bool>("two");
                AddColumn<bool>("three");
                AddColumn<bool>("four");
                AddColumn<string>("Name");
            }
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
            var session = _host.Get<SagaDbContext>();
            return await session.FindAsync<TSagaState>(id);
        }

        protected async Task<TSagaState> LoadState(int id)
        {
            var session = _host.Get<SagaDbContext>();
            return await session.FindAsync<TSagaState>(id);
        }

        protected async Task<TSagaState> LoadState(long id)
        {
            var session = _host.Get<SagaDbContext>();
            return await session.FindAsync<TSagaState>(id);
        }

        protected async Task<TSagaState> LoadState(string id)
        {
            var session = _host.Get<SagaDbContext>();
            return await session.FindAsync<TSagaState>(id);
        }
    }
}
