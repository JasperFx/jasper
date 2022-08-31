// <auto-generated/>
#pragma warning disable
using Jasper.Persistence.Marten.Publishing;

namespace Internal.Generated.JasperHandlers
{
    // START: IncrementManyHandler392445237
    public class IncrementManyHandler392445237 : Jasper.Runtime.Handlers.MessageHandler
    {
        private readonly Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;

        public IncrementManyHandler392445237(Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory)
        {
            _outboxedSessionFactory = outboxedSessionFactory;
        }



        public override async System.Threading.Tasks.Task HandleAsync(Jasper.IMessageContext context, System.Threading.CancellationToken cancellation)
        {
            var letterHandler = new Jasper.Persistence.Testing.Marten.LetterHandler();
            var incrementMany = (Jasper.Persistence.Testing.Marten.IncrementMany)context.Envelope.Message;
            await using var documentSession = _outboxedSessionFactory.OpenSession(context);
            var eventStore = documentSession.Events;
            // Loading Marten aggregate
            var eventStream = await eventStore.FetchForWriting<Jasper.Persistence.Testing.Marten.LetterAggregate>(incrementMany.LetterAggregateId, cancellation).ConfigureAwait(false);

            var outgoing1 = letterHandler.Handle(incrementMany, eventStream.Aggregate, documentSession);
            if (outgoing1 != null)
            {
                // Capturing any possible events returned from the command handlers
                eventStream.AppendMany(outgoing1);

            }

            await documentSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }

    }

    // END: IncrementManyHandler392445237


}

