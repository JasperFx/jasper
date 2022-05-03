using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Durability
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        private readonly IWorkerQueue _workers;
        private readonly AdvancedSettings _settings;
        private readonly ILogger _logger;

        public RecoverIncomingMessages(IWorkerQueue workers, AdvancedSettings settings,
            ILogger logger)
        {
            _workers = workers;
            _settings = settings;
            _logger = logger;
        }

        public async Task ExecuteAsync(IEnvelopePersistence storage, IDurabilityAgent agent)
        {
            // TODO -- enforce back pressure here on the retries listener!

            await storage.Session.BeginAsync();


            var incoming = await determineIncomingAsync(storage);

            _logger.RecoveredIncoming(incoming);

            foreach (var envelope in incoming)
            {
                envelope.OwnerId = _settings.UniqueNodeId;
                await _workers.EnqueueAsync(envelope);
            }

            // TODO -- this should be smart enough later to check for back pressure before rescheduling
            if (incoming.Count == _settings.RecoveryBatchSize)
                agent.RescheduleIncomingRecovery();
        }

        private async Task<IReadOnlyList<Envelope>> determineIncomingAsync(IEnvelopePersistence storage)
        {
            try
            {
                var gotLock = await storage.Session.TryGetGlobalLock(TransportConstants.IncomingMessageLockId);

                if (!gotLock)
                {
                    await storage.Session.RollbackAsync();
                    return new List<Envelope>();
                }

                var incoming = await storage.LoadPageOfGloballyOwnedIncomingAsync();

                if (!incoming.Any())
                {
                    await storage.Session.RollbackAsync();
                    return incoming; // Okay to return the empty list here any way
                }

                await storage.ReassignIncomingAsync(_settings.UniqueNodeId, incoming);

                await storage.Session.CommitAsync();
            }
            catch (Exception)
            {
                await storage.Session.RollbackAsync();
                throw;
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.IncomingMessageLockId);
            }

            return new List<Envelope>();
        }

        public string Description { get; } = "Recover persisted incoming messages";
    }
}
