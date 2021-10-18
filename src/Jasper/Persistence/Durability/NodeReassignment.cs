using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Persistence.Durability
{
    public class NodeReassignment : IMessagingAction
    {
        private readonly AdvancedSettings _settings;

        public NodeReassignment(AdvancedSettings settings)
        {
            _settings = settings;
        }

        public string Description { get; } = "Dormant node reassignment";

        public async Task Execute(IEnvelopePersistence storage, IDurabilityAgent agent)
        {
            await storage.Session.Begin();

            var gotLock = await storage.Session.TryGetGlobalLock(TransportConstants.ReassignmentLockId);
            if (!gotLock)
            {
                await storage.Session.Rollback();
                return;
            }

            try
            {
                var owners = await storage.FindUniqueOwners(_settings.UniqueNodeId);

                foreach (var owner in owners.Where(x => x != TransportConstants.AnyNode))
                {
                    if (owner == _settings.UniqueNodeId) continue;

                    if (await storage.Session.TryGetGlobalTxLock(owner))
                    {
                        await storage.ReassignDormantNodeToAnyNode(owner);
                    }
                }
            }
            catch (Exception)
            {
                await storage.Session.Rollback();
                throw;
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.ReassignmentLockId);
            }

            await storage.Session.Commit();
        }


    }


}
