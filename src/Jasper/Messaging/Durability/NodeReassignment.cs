using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Durability
{
    public class NodeReassignment : IMessagingAction
    {
        private readonly JasperOptions _options;
        private readonly ITransportLogger _logger;

        public NodeReassignment(JasperOptions options, ITransportLogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public string Description { get; } = "Dormant node reassignment";

        public async Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
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
                var owners = await storage.Nodes.FindUniqueOwners(_options.UniqueNodeId);

                foreach (var owner in owners.Where(x => x != TransportConstants.AnyNode))
                {
                    if (owner == _options.UniqueNodeId) continue;

                    if (await storage.Session.TryGetGlobalTxLock(owner))
                    {
                        await storage.Nodes.ReassignDormantNodeToAnyNode(owner);
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
