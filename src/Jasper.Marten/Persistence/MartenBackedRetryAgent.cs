using System.Linq;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.Tcp;
using Jasper.Marten.Persistence.Resiliency;
using Marten;

namespace Jasper.Marten.Persistence
{
    public class MartenBackedRetryAgent : RetryAgent
    {
        private readonly IDocumentStore _store;
        private readonly OwnershipMarker _marker;

        public MartenBackedRetryAgent(IDocumentStore store, ISender sender, RetrySettings settings, OwnershipMarker marker) : base(sender, settings)
        {
            _store = store;
            _marker = marker;
        }

        public override void EnqueueForRetry(OutgoingMessageBatch batch)
        {
            using (var session = _store.LightweightSession())
            {
                _marker.MarkOwnedByAnyNode(session, batch.Messages.ToArray());
                session.SaveChanges();
            }
        }

        protected override void afterRestarting()
        {
            // Nothing here.
        }
    }
}