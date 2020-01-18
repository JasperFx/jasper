using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Marten;
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

namespace StorytellerSpecs.Fixtures.Marten
{
    public class MartenDurableFixture : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        public MartenDurableFixture()
        {
            Title = "Marten Outbox & Scheduled Message Mechanics";
        }

        protected override void configureReceiver(JasperOptions receiverOptions)
        {
            receiverOptions.Extensions.UseMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_receiver";
            });
        }

        protected override void configureSender(JasperOptions senderOptions)
        {
            senderOptions.UseMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_sender";
            });
        }


        protected override void initializeStorage(IHost theSender, IHost theReceiver)
        {
            theSender.RebuildMessageStorage();

            theReceiver.RebuildMessageStorage();
        }

        protected override ItemCreated loadItem(IHost receiver, Guid id)
        {
            using (var session = receiver.Get<IDocumentStore>().QuerySession())
            {
                return session.Load<ItemCreated>(id);
            }
        }


        protected override async Task withContext(IHost sender, IMessageContext context,
            Func<IMessageContext, Task> action)
        {
            var senderStore = sender.Get<IDocumentStore>();

            using (var session = senderStore.LightweightSession())
            {
                await context.EnlistInTransaction(session);

                await action(context);

                await session.SaveChangesAsync();
            }
        }

        protected override Envelope[] loadAllOutgoingEnvelopes(IHost sender)
        {
            var admin = sender.Get<IEnvelopePersistence>().Admin;
            return admin.AllOutgoingEnvelopes().GetAwaiter().GetResult();
        }
    }

    public class TriggerMessageReceiver
    {
        [Transactional]
        public Task Handle(TriggerMessage message, IDocumentSession session, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return context.RespondToSender(response);
        }
    }

    public class ItemCreatedHandler
    {
        [Transactional]
        public static void Handle(ItemCreated created, IDocumentSession session,
            Envelope envelope)
        {
            session.Store(created);
        }
    }
}
