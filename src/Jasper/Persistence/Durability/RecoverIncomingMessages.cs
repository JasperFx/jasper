using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;

namespace Jasper.Persistence.Durability
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        private readonly IEnvelopePersistence _persistence;
        private readonly IWorkerQueue _workers;
        private readonly AdvancedSettings _settings;
        private readonly ITransportLogger _logger;

        public RecoverIncomingMessages(IEnvelopePersistence persistence, IWorkerQueue workers, AdvancedSettings settings,
            ITransportLogger logger)
        {
            _persistence = persistence;
            _workers = workers;
            this._settings = settings;
            _logger = logger;
        }

        public async Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
        {
            // TODO -- enforce back pressure here on the retries listener!

            await storage.Session.Begin();


            Envelope[] incoming = null;
            try
            {
                var gotLock = await storage.Session.TryGetGlobalLock(TransportConstants.IncomingMessageLockId);

                if (!gotLock)
                {
                    await storage.Session.Rollback();
                    return;
                }

                incoming = await storage.Incoming.LoadPageOfLocallyOwned();

                if (!incoming.Any())
                {
                    await storage.Session.Rollback();
                    return;
                }

                await storage.Incoming.Reassign(_settings.UniqueNodeId, incoming);

                await storage.Session.Commit();
            }
            catch (Exception)
            {
                await storage.Session.Rollback();
                throw;
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.IncomingMessageLockId);
            }

            _logger.RecoveredIncoming(incoming);

            foreach (var envelope in incoming)
            {
                envelope.OwnerId = _settings.UniqueNodeId;
                await _workers.Enqueue(envelope);
            }

            // TODO -- this should be smart enough later to check for back pressure before rescheduling
            if (incoming.Length == _settings.RecoveryBatchSize)
                agent.RescheduleIncomingRecovery();
        }

        public string Description { get; } = "Recover persisted incoming messages";
    }
}
