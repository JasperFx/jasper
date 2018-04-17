using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Marten.Persistence.Operations;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.WorkerQueues;
using Marten;
using Marten.Util;

namespace Jasper.Marten.Persistence.Resiliency
{
    public class RecoverIncomingMessages : IMessagingAction
    {
        public static readonly int IncomingMessageLockId = "recover-incoming-messages".GetHashCode();
        private readonly ITransportLogger _logger;
        private readonly EnvelopeTables _marker;
        private readonly ISchedulingAgent _schedulingAgent;
        private readonly MessagingSettings _settings;
        private readonly IWorkerQueue _workers;
        private readonly string _findAtLargeEnvelopesSql;

        public RecoverIncomingMessages(IWorkerQueue workers, MessagingSettings settings, EnvelopeTables marker,
            ISchedulingAgent schedulingAgent, ITransportLogger logger)
        {
            _workers = workers;
            _settings = settings;
            _marker = marker;
            _schedulingAgent = schedulingAgent;
            _logger = logger;

            _findAtLargeEnvelopesSql = $"select body from {marker.Incoming} where owner_id = {TransportConstants.AnyNode} and status = '{TransportConstants.Incoming}' limit {settings.Retries.RecoveryBatchSize}";
        }

        public async Task Execute(IDocumentSession session)
        {
            if (!await session.TryGetGlobalTxLock(IncomingMessageLockId))
                return;


            var incoming = await session.Connection.CreateCommand(_findAtLargeEnvelopesSql)
                .ExecuteToEnvelopes();

            if (!incoming.Any()) return;

            session.MarkOwnership(_marker.Incoming, _settings.UniqueNodeId, incoming);

            await session.SaveChangesAsync();

            _logger.RecoveredIncoming(incoming);

            foreach (var envelope in incoming)
            {
                envelope.OwnerId = _settings.UniqueNodeId;
                await _workers.Enqueue(envelope);
            }

            if (incoming.Count == _settings.Retries.RecoveryBatchSize &&
                _workers.QueuedCount < _settings.MaximumLocalEnqueuedBackPressureThreshold)
            {
                _schedulingAgent.RescheduleIncomingRecovery();
            }
        }
    }
}
