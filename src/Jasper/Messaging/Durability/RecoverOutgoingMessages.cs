using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Util;

namespace Jasper.Messaging.Durability
{
    public class RecoverOutgoingMessages : IMessagingAction
    {
        private readonly ITransportRuntime _runtime;
        private readonly AdvancedSettings _settings;
        private readonly ITransportLogger _logger;

        public RecoverOutgoingMessages(ITransportRuntime runtime, AdvancedSettings settings, ITransportLogger logger)
        {
            _runtime = runtime;
            _settings = settings;
            _logger = logger;
        }

        public string Description { get; } = "Recover persisted outgoing messages";
        public async Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
        {
            var hasLock = await storage.Session.TryGetGlobalLock(TransportConstants.OutgoingMessageLockId);
            if (!hasLock) return;

            try
            {
                var destinations = await storage.Outgoing.FindAllDestinations();

                var count = 0;
                foreach (var destination in destinations)
                {
                    var found = await recoverFrom(destination, storage);

                    count += found;
                }

                var wasMaxedOut = count >= _settings.RecoveryBatchSize;

                if (wasMaxedOut) agent.RescheduleOutgoingRecovery();
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.OutgoingMessageLockId);
            }
        }



        private async Task<int> recoverFrom(Uri destination, IDurabilityAgentStorage storage)
        {
            try
            {
                Envelope[] filtered = null;
                Envelope[] outgoing = null;

                if (_runtime.GetOrBuildSendingAgent(destination).Latched) return 0;

                await storage.Session.Begin();

                try
                {
                    outgoing = await storage.Outgoing.Load(destination: destination);

                    var expiredMessages = outgoing.Where(x => x.IsExpired()).ToArray();
                    _logger.DiscardedExpired(expiredMessages);


                    await storage.Outgoing.Delete(expiredMessages.ToArray());
                    filtered = outgoing.Where(x => !expiredMessages.Contains(x)).ToArray();

                    // Might easily try to do this in the time between starting
                    // and having the data fetched. Was able to make that happen in
                    // (contrived) testing
                    if (_runtime.GetOrBuildSendingAgent(destination).Latched || !filtered.Any())
                    {
                        await storage.Session.Rollback();
                        return 0;
                    }

                    await storage.Outgoing.Reassign(_settings.UniqueNodeId, filtered);


                    await storage.Session.Commit();
                }
                catch (Exception)
                {
                    await storage.Session.Rollback();
                    throw;
                }

                _logger.RecoveredOutgoing(filtered);

                foreach (var envelope in filtered)
                    try
                    {
                        await _runtime.GetOrBuildSendingAgent(destination).EnqueueOutgoing(envelope);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e, message: $"Unable to enqueue {envelope} for sending");
                    }

                return outgoing.Count();
            }
            catch (UnknownTransportException e)
            {
                _logger.LogException(e, message: $"Could not resolve a channel for {destination}");

                await storage.Session.Begin();

                await storage.Outgoing.DeleteByDestination(destination);
                await storage.Session.Commit();

                return 0;
            }
        }
    }
}
