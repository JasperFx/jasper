// <auto-generated/>
#pragma warning disable
using Jasper.Persistence.Marten.Publishing;

namespace Internal.Generated.JasperHandlers
{
    // START: IncrementCDHandler356715529
    public class IncrementCDHandler356715529 : Jasper.Runtime.Handlers.MessageHandler
    {
        private readonly Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory _outboxedSessionFactory;

        public IncrementCDHandler356715529(Jasper.Persistence.Marten.Publishing.OutboxedSessionFactory outboxedSessionFactory)
        {
            _outboxedSessionFactory = outboxedSessionFactory;
        }



        public override async System.Threading.Tasks.Task HandleAsync(Jasper.IExecutionContext context, System.Threading.CancellationToken cancellation)
        {
            var letterHandler = new Jasper.Persistence.Testing.Marten.LetterHandler();
            var incrementCD = (Jasper.Persistence.Testing.Marten.IncrementCD)context.Envelope.Message;
            await using var documentSession = _outboxedSessionFactory.OpenSession(context);
            var eventStore = documentSession.Events;
            // Loading Marten aggregate
            var eventStream = await eventStore.FetchForWriting<Jasper.Persistence.Testing.Marten.LetterAggregate>(incrementCD.LetterAggregateId, cancellation).ConfigureAwait(false);

            var outgoing1 = letterHandler.Handle(incrementCD, eventStream.Aggregate);
            if (outgoing1 != null)
            {
                // Capturing any possible events returned from the command handlers
                eventStream.AppendMany(outgoing1);

            }

            await documentSession.SaveChangesAsync(cancellation).ConfigureAwait(false);
        }

    }

    // END: IncrementCDHandler356715529
    
    
}

