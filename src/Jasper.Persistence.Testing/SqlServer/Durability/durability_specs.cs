using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Weasel.Core;
using Xunit;
using CommandExtensions = Weasel.Core.CommandExtensions;

namespace Jasper.Persistence.Testing.SqlServer.Durability
{
    [Collection("sqlserver")]
    public class durability_specs : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        protected override async Task initializeStorage(IHost sender, IHost receiver)
        {
            await sender.RebuildMessageStorage();
            await receiver.RebuildMessageStorage();


            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                await conn.CreateCommand(@"
IF OBJECT_ID('receiver.item_created', 'U') IS NOT NULL
  drop table receiver.item_created;

").ExecuteNonQueryAsync();

                await conn.CreateCommand(@"
create table receiver.item_created
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQueryAsync();
            }
        }

        protected override void configureReceiver(JasperOptions receiverOptions)
        {
            receiverOptions.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "receiver");
        }

        protected override void configureSender(JasperOptions senderOptions)
        {
            senderOptions.Extensions.PersistMessagesWithSqlServer(Servers.SqlServerConnectionString, "sender");
        }

        protected override ItemCreated loadItem(IHost receiver, Guid id)
        {
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                var name = (string) CommandExtensions.CreateCommand(conn, "select name from receiver.item_created where id = @id")
                    .With("id", id)
                    .ExecuteScalar();

                if (name.IsEmpty()) return null;

                return new ItemCreated
                {
                    Id = id,
                    Name = name
                };
            }
        }

        protected override async Task withContext(IHost sender, IExecutionContext context,
            Func<IExecutionContext, Task> action)
        {
            // SAMPLE: basic-sql-server-outbox-sample
            await using var conn = new SqlConnection(Servers.SqlServerConnectionString);
            await conn.OpenAsync();

            var tx = conn.BeginTransaction();

            // "context" is an IMessageContext object
            await context.EnlistInTransaction(tx);

            await action(context);

            tx.Commit();

            await context.SendAllQueuedOutgoingMessages();

            // ENDSAMPLE
        }

        protected override IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender)
        {
            return sender.Get<IEnvelopePersistence>().As<SqlServerEnvelopePersistence>()
                .Admin.AllOutgoingEnvelopes().GetAwaiter().GetResult();
        }
    }

    // SAMPLE: UsingSqlTransaction
    // ENDSAMPLE
}
