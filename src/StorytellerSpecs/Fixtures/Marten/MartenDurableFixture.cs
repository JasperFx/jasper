using System;
using System.Threading.Tasks;
using IntegrationTests;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Persistence;
using Jasper.Persistence.Marten.Persistence.Operations;
using Marten;
using StorytellerSpecs.Fixtures.Durability;

namespace StorytellerSpecs.Fixtures.Marten
{
    public class MartenDurableFixture : DurableFixture<TriggerMessageReceiver, ItemCreatedHandler>
    {
        public MartenDurableFixture()
        {
            Title = "Marten Outbox & Scheduled Message Mechanics";
        }

        protected override void configureReceiver(JasperRegistry receiverRegistry)
        {
            receiverRegistry.Settings.ConfigureMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_receiver";
            });

            receiverRegistry.Include<MartenBackedPersistence>();
        }

        protected override void configureSender(JasperRegistry senderRegistry)
        {
            senderRegistry.Settings.ConfigureMarten(marten =>
            {
                marten.Connection(Servers.PostgresConnectionString);
                marten.DatabaseSchemaName = "outbox_sender";
            });

            senderRegistry.Include<MartenBackedPersistence>();
        }


        protected override void initializeStorage(JasperRuntime theSender, JasperRuntime theReceiver)
        {
            var senderStore = theSender.Get<IDocumentStore>();
            senderStore.Advanced.Clean.CompletelyRemoveAll();
            senderStore.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            theSender.Get<MartenBackedDurableMessagingFactory>().ClearAllStoredMessages();

            var receiverStore = theReceiver.Get<IDocumentStore>();
            receiverStore.Advanced.Clean.CompletelyRemoveAll();
            receiverStore.Tenancy.Default.EnsureStorageExists(typeof(Envelope));

            theReceiver.Get<MartenBackedDurableMessagingFactory>().ClearAllStoredMessages();
        }

        protected override ItemCreated loadItem(JasperRuntime receiver, Guid id)
        {
            using (var session = receiver.Get<IDocumentStore>().QuerySession())
            {
                return session.Load<ItemCreated>(id);
            }
        }


        protected override async Task withContext(JasperRuntime sender, IMessageContext context,
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

        protected override Envelope[] loadAllOutgoingEnvelopes(JasperRuntime sender)
        {
            using (var session = sender.Get<IDocumentStore>().QuerySession())
            {
                return session.AllOutgoingEnvelopes().ToArray();
            }
        }
    }

    public class TriggerMessageReceiver
    {
        [Transactional]
        public object Handle(TriggerMessage message, IDocumentSession session, IMessageContext context)
        {
            var response = new CascadedMessage
            {
                Name = message.Name
            };

            return new RespondToSender(response);
        }
    }

    public class ItemCreatedHandler
    {
        [Transactional]
        public static void Handle(ItemCreated created, IDocumentSession session,
            Jasper.Messaging.Tracking.MessageTracker tracker,
            Envelope envelope)
        {
            session.Store(created);
            tracker.Record(created, envelope);
        }
    }
}
