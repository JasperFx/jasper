using System;
using System.Threading.Tasks;
using Marten;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class ReassignFromDormantNodes : IMessagingAction
    {
        public readonly int ReassignmentLockId = "jasper-reassign-envelopes".GetHashCode();

        public ReassignFromDormantNodes(StoreOptions options)
        {
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(ReassignmentLockId))
                return;

            // TODO -- find all nodes reflected in Envelope.OwnerId where you
            // can get an advisory lock, which tells us that that node is down. Reassign all to AnyNode
            =
            // think this needs to be a sproc w/ a cursor. Boo.
            throw new NotImplementedException();
        }
    }
}
