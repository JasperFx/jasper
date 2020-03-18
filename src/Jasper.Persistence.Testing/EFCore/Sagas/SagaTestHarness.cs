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
using Xunit.Abstractions;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    public abstract class SagaTestHarness<TSagaHandler, TSagaState> : SqlServerContext, IDisposable
        where TSagaHandler : StatefulSagaOf<TSagaState> where TSagaState : class
    {
        private readonly IHost _host;

        protected SagaTestHarness(ITestOutputHelper output)
        {
            _host = JasperHost.For(_ =>
            {
                _.Handlers.DisableConventionalDiscovery().IncludeType<TSagaHandler>();

                _.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString);

                _.Services.AddDbContext<SagaDbContext>(x => x.UseSqlServer(Servers.SqlServerConnectionString));

                _.Extensions.Include<EntityFrameworkCoreBackedPersistence>();

                _.Extensions.Include<MessageTrackingExtension>();

                _.Endpoints.PublishAllMessages().Locally();

                configure(_);
            });


            rebuildSagaStateDatabase();

            _host.RebuildMessageStorage();

//            var builder = _host.Get<DynamicCodeBuilder>();
//            builder.TryBuildAndCompileAll((a, s) => {});
//
//            var code = builder.GenerateAllCode();
//
//            File.WriteAllText("SagaCode.cs", code);
        }

        public void Dispose()
        {
            _host?.Dispose();
        }

        private void rebuildSagaStateDatabase()
        {
            using var conn = new SqlConnection(Servers.SqlServerConnectionString);
            conn.Open();

            dropTable(conn, "GuidWorkflowState");
            dropTable(conn, "IntWorkflowState");
            dropTable(conn, "LongWorkflowState");
            dropTable(conn, "StringWorkflowState");

            conn.RunSql(@"
CREATE TABLE [GuidWorkflowState] (
    [Id] uniqueidentifier NOT NULL,
    [one] bit NOT NULL,
    [two] bit NOT NULL,
    [three] bit NOT NULL,
    [four] bit NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_GuidWorkflowState] PRIMARY KEY ([Id])
);
");
            conn.RunSql(@"
CREATE TABLE [IntWorkflowState] (
    [Id] int NOT NULL,
    [one] bit NOT NULL,
    [two] bit NOT NULL,
    [three] bit NOT NULL,
    [four] bit NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_IntWorkflowState] PRIMARY KEY ([Id])
);");


            conn.RunSql(@"
CREATE TABLE [LongWorkflowState] (
    [Id] bigint NOT NULL,
    [one] bit NOT NULL,
    [two] bit NOT NULL,
    [three] bit NOT NULL,
    [four] bit NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_LongWorkflowState] PRIMARY KEY ([Id])
);");


            conn.RunSql(@"
CREATE TABLE [StringWorkflowState] (
    [Id] nvarchar(450) NOT NULL,
    [one] bit NOT NULL,
    [two] bit NOT NULL,
    [three] bit NOT NULL,
    [four] bit NOT NULL,
    [Name] nvarchar(max) NULL,
    CONSTRAINT [PK_StringWorkflowState] PRIMARY KEY ([Id])
);
");
        }

        private void dropTable(SqlConnection conn, string tableName)
        {
            conn.RunSql($"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL drop table {tableName};");
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
