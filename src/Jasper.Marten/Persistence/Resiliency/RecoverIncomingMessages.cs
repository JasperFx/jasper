using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.WorkerQueues;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        public static readonly int IncomingMessageLockId = "recover-incoming-messages".GetHashCode();
        private readonly IWorkerQueue _workers;
        private readonly BusSettings _settings;
        private readonly OwnershipMarker _marker;
        private readonly ISchedulingAgent _schedulingAgent;

        public RecoverIncomingMessages(IWorkerQueue workers, BusSettings settings, OwnershipMarker marker, ISchedulingAgent schedulingAgent)
        {
            _workers = workers;
            _settings = settings;
            _marker = marker;
            _schedulingAgent = schedulingAgent;
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(IncomingMessageLockId))
            {
                return;
            }


            // Have it loop until either a time limit has been reached or a certain
            // queue count has been reached
            // TODO -- how many do you pull in here?

            // TODO -- make this a compiled query
            var incoming = await session.Query<Envelope>()
                .Where(x => x.OwnerId == TransportConstants.AnyNode && x.Status == TransportConstants.Incoming)
                .Take(100) // TODO -- should this be configurable?
                .ToListAsync();

            await _marker.MarkIncomingOwnedByThisNode(session, incoming.ToArray());

            await session.SaveChangesAsync();

            foreach (var envelope in incoming)
            {
                envelope.OwnerId = _settings.UniqueNodeId;
                await _workers.Enqueue(envelope);
            }
        }
    }
}
