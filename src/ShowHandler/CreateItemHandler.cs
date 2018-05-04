using System.Threading.Tasks;
using Jasper.Marten;
using Jasper.Messaging;
using Marten;

namespace ShowHandler
{
    /*
    public class CreateItemHandler
    {
        private readonly IDocumentStore _store;

        public CreateItemHandler(IDocumentStore store)
        {
            _store = store;
        }

        public ItemCreatedEvent Handle(CreateItemCommand command)
        {
            using (var session = _store.LightweightSession())
            {
                var item = new Item {Name = command.Name};
                session.Store(item);
                session.SaveChanges();

                return new ItemCreatedEvent {Item = item};
            }
        }
    }
    */

    /*
    public class CreateItemHandler
    {
        private readonly IDocumentSession _session;

        public CreateItemHandler(IDocumentSession session)
        {
            _session = session;
        }

        public void Handle(CreateItemCommand command)
        {
            var item = new Item {Name = command.Name};
            _session.Store(item);
            _session.SaveChanges();
        }
    }
    */


    public static class ExplicitOutboxUsage
    {
        // SAMPLE: ExplicitOutboxUsage
        public static async Task explicit_usage(
            CreateItemCommand command,
            IDocumentStore store,
            IMessageContext context)
        {
            var item = new Item {Name = command.Name};

            using (var session = store.LightweightSession())
            {
                // This tells the current Jasper message context
                // to persist outgoing messages with this
                // Marten session and to delay actually sending
                // out the outgoing messages until the session
                // has been committed
                await context.EnlistInTransaction(session);

                session.Store(item);

                // Registering an outgoing message, nothing is
                // actually "sent" yet though
                await context.Publish(new ItemCreatedEvent {Item = item});

                // Commit all the queued up document and outgoing messages
                // to the underlying Postgresql database.
                await session.SaveChangesAsync();
            }
        }
        // ENDSAMPLE
    }

    // SAMPLE: MartenCreateItemHandler
    public class CreateItemHandler
    {
        [MartenTransaction]
        public static ItemCreatedEvent Handle(CreateItemCommand command, IDocumentSession session)
        {
            // This Item document and the outgoing ItemCreatedEvent
            // message being published will be persisted in
            // the same native Postgresql transaction
            var item = new Item {Name = command.Name};
            session.Store(item);

            // The outgoing message here is persisted with Marten
            // as part of the same transaction
            return new ItemCreatedEvent{Item = item};
        }
    }
    // ENDSAMPLE
}
