using System;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Persistence;
using Jasper.Persistence.Database;
using Jasper.Persistence.Postgresql;
using Npgsql;
using StorytellerSpecs.Fixtures.Durability;

namespace StorytellerSpecs.Fixtures.Postgresql
{
    public class PostgresqlDurableFixture : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        public PostgresqlDurableFixture()
        {
            Title = "Postgresql-Only Outbox & Scheduled Message Mechanics";
        }

        protected override void configureReceiver(JasperRegistry receiverRegistry)
        {
            receiverRegistry.Settings.PersistMessagesWithPostgresql(Servers.PostgresConnectionString, "outbox_receiver");
        }

        protected override void configureSender(JasperRegistry senderRegistry)
        {
            senderRegistry.Settings.PersistMessagesWithPostgresql(Servers.PostgresConnectionString, "outbox_sender");
        }


        protected override void initializeStorage(IJasperHost theSender, IJasperHost theReceiver)
        {
            theSender.RebuildMessageStorage();

            theReceiver.RebuildMessageStorage();

            using (var conn = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                conn.Open();

                conn.CreateCommand(@"
create table if not exists receiver.item_created
(
	id uuid not null primary key,
	name varchar(100) not null
)
").ExecuteNonQuery();
            }
        }

        protected override ItemCreated loadItem(IJasperHost receiver, Guid id)
        {
            using (var conn = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                conn.Open();

                var name = (string) conn.CreateCommand("select name from receiver.item_created where id = @id")
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


        protected override async Task withContext(IJasperHost sender, IMessageContext context,
            Func<IMessageContext, Task> action)
        {
            // SAMPLE: basic-postgresql-outbox-sample
            using (var conn = new NpgsqlConnection(Servers.PostgresConnectionString))
            {
                await conn.OpenAsync();

                var tx = conn.BeginTransaction();

                // "context" is an IMessageContext object
                await context.EnlistInTransaction(tx);

                await action(context);

                tx.Commit();

                await context.SendAllQueuedOutgoingMessages();
            }

            // ENDSAMPLE
        }

        protected override Envelope[] loadAllOutgoingEnvelopes(IJasperHost sender)
        {
            var admin = sender.Get<IEnvelopePersistence>().Admin;
            return admin.AllOutgoingEnvelopes().GetAwaiter().GetResult();
        }
    }

    public class TriggerMessageReceiver
    {
        [Transactional]
        public object Handle(TriggerMessage message, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return new RespondToSender(response);
        }
    }

    // SAMPLE: UsingNpgsqlTransaction
    public class ItemCreatedHandler
    {
        [Transactional]
        public static async Task Handle(
            ItemCreated created,
            NpgsqlTransaction tx, // the current transaction
            Jasper.Messaging.Tracking.MessageTracker tracker,
            Envelope envelope)
        {
            // Using some extension method helpers inside of Jasper here
            await tx.CreateCommand("insert into receiver.item_created (id, name) values (@id, @name)")
                .With("id", created.Id)
                .With("name", created.Name)
                .ExecuteNonQueryAsync();

            tracker.Record(created, envelope);
        }
    }
    // ENDSAMPLE
}
