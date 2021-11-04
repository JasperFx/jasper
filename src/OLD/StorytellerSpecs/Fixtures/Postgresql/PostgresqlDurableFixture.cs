using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Postgresql;
using Microsoft.Extensions.Hosting;
using Npgsql;
using StorytellerSpecs.Fixtures.Durability;
using Weasel.Core;

namespace StorytellerSpecs.Fixtures.Postgresql
{
    public class PostgresqlDurableFixture : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        public PostgresqlDurableFixture()
        {
            Title = "Postgresql-Only Outbox & Scheduled Message Mechanics";
        }

        protected override void configureReceiver(JasperOptions receiverOptions)
        {
            receiverOptions.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString,
                "outbox_receiver");
        }

        protected override void configureSender(JasperOptions senderOptions)
        {
            senderOptions.Extensions.PersistMessagesWithPostgresql(Servers.PostgresConnectionString, "outbox_sender");
        }


        protected override void initializeStorage(IHost theSender, IHost theReceiver)
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

        protected override ItemCreated loadItem(IHost receiver, Guid id)
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


        protected override async Task withContext(IHost sender, IExecutionContext context,
            Func<IExecutionContext, Task> action)
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

        protected override IReadOnlyList<Envelope> loadAllOutgoingEnvelopes(IHost sender)
        {
            var admin = sender.Get<IEnvelopePersistence>().Admin;
            return admin.AllOutgoingEnvelopes().GetAwaiter().GetResult();
        }
    }

    public class TriggerMessageReceiver
    {
        [Transactional]
        public Task Handle(TriggerMessage message, IExecutionContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return context.RespondToSender(response);
        }
    }

    // SAMPLE: UsingNpgsqlTransaction
    public class ItemCreatedHandler
    {
        [Transactional]
        public static async Task Handle(
            ItemCreated created,
            NpgsqlTransaction tx, // the current transaction
            Envelope envelope)
        {
            // Using some extension method helpers inside of Jasper here
            await tx.CreateCommand("insert into receiver.item_created (id, name) values (@id, @name)")
                .With("id", created.Id)
                .With("name", created.Name)
                .ExecuteNonQueryAsync();
        }
    }
    // ENDSAMPLE

    public class CreateItemHandler
    {
        // SAMPLE: PostgresqlOutboxWithNpgsqlTransaction
        [Transactional]
        public async Task<ItemCreatedEvent> Handle(CreateItemCommand command, NpgsqlTransaction tx)
        {
            var item = new Item {Name = command.Name};

            // persist the new Item with the
            // current transaction
            await persist(tx, item);

            return new ItemCreatedEvent {Item = item};
        }
        // ENDSAMPLE

        private Task persist(NpgsqlTransaction tx, Item item)
        {
            // whatever you do to write the new item
            // to your sql server application database
            return Task.CompletedTask;
        }


        public class CreateItemCommand
        {
            public string Name { get; set; }
        }

        public class ItemCreatedEvent
        {
            public Item Item { get; set; }
        }

        public class Item
        {
            public Guid Id;
            public string Name;
        }
    }
}
