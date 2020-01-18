using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence;
using Jasper.Persistence.Database;
using Jasper.Persistence.Durability;
using Jasper.Persistence.SqlServer;
using Jasper.Persistence.SqlServer.Persistence;
using StorytellerSpecs.Fixtures.Durability;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace StorytellerSpecs.Fixtures.SqlServer
{
    public class SqlServerDurableFixture : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        public SqlServerDurableFixture()
        {
            Title = "Sql Server Outbox & Scheduled Message Mechanics";
        }

        protected override void initializeStorage(IHost sender, IHost receiver)
        {
            sender.RebuildMessageStorage();
            receiver.RebuildMessageStorage();


            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
            {
                conn.Open();

                conn.CreateCommand(@"
IF OBJECT_ID('receiver.item_created', 'U') IS NOT NULL
  drop table receiver.item_created;

").ExecuteNonQuery();

                conn.CreateCommand(@"
create table receiver.item_created
(
	id uniqueidentifier not null
		primary key,
	name varchar(100) not null
);

").ExecuteNonQuery();
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

        protected override async Task withContext(IHost sender, IMessageContext context,
            Func<IMessageContext, Task> action)
        {
            // SAMPLE: basic-sql-server-outbox-sample
            using (var conn = new SqlConnection(Servers.SqlServerConnectionString))
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

        protected override Envelope[] loadAllOutgoingEnvelopes(IHost sender)
        {
            return sender.Get<IEnvelopePersistence>().As<SqlServerEnvelopePersistence>()
                .AllOutgoingEnvelopes().ToArray();
        }
    }

    public class TriggerMessageReceiver
    {
        [Transactional]
        public Task Handle(TriggerMessage message, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return context.RespondToSender(response);
        }
    }

    // SAMPLE: UsingSqlTransaction
    public class ItemCreatedHandler
    {
        [Transactional]
        public static async Task Handle(
            ItemCreated created,
            SqlTransaction tx // the current transaction
        )
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
        // SAMPLE: SqlServerOutboxWithSqlTransaction
        [Transactional]
        public async Task<ItemCreatedEvent> Handle(CreateItemCommand command, SqlTransaction tx)
        {
            var item = new Item {Name = command.Name};

            // persist the new Item with the
            // current transaction
            await persist(tx, item);

            return new ItemCreatedEvent {Item = item};
        }
        // ENDSAMPLE

        private Task persist(SqlTransaction tx, Item item)
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
