using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Durability
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        private readonly IEnvelopePersistence _persistence;
        private readonly IWorkerQueue _workers;
        private readonly JasperOptions _options;
        private readonly ITransportLogger _logger;

        public RecoverIncomingMessages(IEnvelopePersistence persistence, IWorkerQueue workers, JasperOptions options,
            ITransportLogger logger)
        {
            _persistence = persistence;
            _workers = workers;
            _options = options;
            _logger = logger;
        }

        public async Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
        {
            if (_workers.QueuedCount > _options.MaximumLocalEnqueuedBackPressureThreshold) return;

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

                await storage.Incoming.Reassign(_options.UniqueNodeId, incoming);

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
                envelope.OwnerId = _options.UniqueNodeId;
                envelope.Callback = new DurableCallback(envelope, _workers, _persistence,_logger);
                await _workers.Enqueue(envelope);
            }

            if (incoming.Length == _options.Retries.RecoveryBatchSize &&
                _workers.QueuedCount < _options.MaximumLocalEnqueuedBackPressureThreshold)
                agent.RescheduleIncomingRecovery();
        }

        public string Description { get; } = "Recover persisted incoming messages";
    }
}
