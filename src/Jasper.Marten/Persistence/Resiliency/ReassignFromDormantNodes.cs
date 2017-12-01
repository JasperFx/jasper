using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Marten;
using Marten.Util;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        private readonly OwnershipMarker _marker;
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();

        public ReassignFromDormantNodes(OwnershipMarker marker)
        {
            _marker = marker;
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(ReassignmentLockId))
            {
                return;
            }

            await _marker.ReassignEnvelopesFromDormantNodes(session);
            await session.SaveChangesAsync();
        }
    }
}
