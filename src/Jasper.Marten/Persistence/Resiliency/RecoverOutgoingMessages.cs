using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        private readonly IChannelGraph _channels;
        private readonly BusSettings _settings;
        private readonly OwnershipMarker _marker;
        private readonly ISchedulingAgent _schedulingAgent;
        public static readonly int OutgoingMessageLockId = "recover-incoming-messages".GetHashCode();

        public RecoverOutgoingMessages(IChannelGraph channels, BusSettings settings, OwnershipMarker marker, ISchedulingAgent schedulingAgent)
        {
            _channels = channels;
            _settings = settings;
            _marker = marker;
            _schedulingAgent = schedulingAgent;
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(OutgoingMessageLockId))
            {
                return;
            }


            // Have it loop until either a time limit has been reached or a certain
            // queue count has been reached
            // TODO -- how many do you pull in here?

            // TODO -- take advantage of channels that are latched

            // TODO -- make this a compiled query
            var outgoing = await session.Query<Envelope>()
                .Where(x => x.OwnerId == TransportConstants.AnyNode && x.Status == TransportConstants.Outgoing)
                .Take(100) // TODO -- should this be configurable?
                .ToListAsync();

            await _marker.MarkOutgoingOwnedByThisNode(session, outgoing.ToArray());

            await session.SaveChangesAsync();

            // TODO -- send them to the





        }
    }
}
