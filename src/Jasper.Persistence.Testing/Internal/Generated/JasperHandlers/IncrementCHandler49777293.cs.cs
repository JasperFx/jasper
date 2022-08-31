// <auto-generated/>
#pragma warning disable
using Jasper.Persistence.Marten.Publishing;

namespace Internal.Generated.JasperHandlers
{
    // START: IncrementCHandler49777293
    public class IncrementCHandler49777293 : Jasper.Runtime.Handlers.MessageHandler
    {
        private readonly Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;

        public IncrementCHandler49777293(Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory)
        {
            _outboxedSessionFactory = outboxedSessionFactory;
        }



        public override async System.Threading.Tasks.Task HandleAsync(Jasper.IMessageContext context, System.Threading.CancellationToken cancellation)
        {
            var letterHandler = new Jasper.Persistence.Testing.Marten.LetterHandler();
            var incrementC = (Jasper.Persistence.Testing.Marten.IncrementC)context.Envelope.Message;
            await using var documentSession = _outboxedSessionFactory.OpenSession(context);
            var eventStore = documentSession.Events;
            // Loading Marten aggregate
            var eventStream = await eventStore.FetchForWriting<Jasper.Persistence.Testing.Marten.LetterAggregate>(incrementC.LetterAggregateId, cancellation).ConfigureAwait(false);

            letterHandler.Handle(incrementC, eventStream);
            await documentSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }

    }

    // END: IncrementCHandler49777293


}
