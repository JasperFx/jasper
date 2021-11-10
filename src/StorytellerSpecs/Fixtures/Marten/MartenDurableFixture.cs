using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper;
using Jasper.Attributes;
using Jasper.Persistence;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Marten;
using Microsoft.Extensions.Hosting;
using StorytellerSpecs.Fixtures.Durability;

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


        protected override async Task withContext(IHost sender, IExecutionContext context,
            Func<IExecutionContext, Task> action)
        {
            var senderStore = sender.Get<IDocumentStore>();

            using (var session = senderStore.LightweightSession())
            {
                await context.EnlistInTransaction(session);

                await action(context);

                await session.SaveChangesAsync();
            }
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
        public Task Handle(TriggerMessage message, IDocumentSession session, IExecutionContext context)
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
