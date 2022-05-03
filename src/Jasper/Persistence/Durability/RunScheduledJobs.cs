using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Durability
{
    internal class RunScheduledJobs : IMessagingAction
    {
        private readonly AdvancedSettings _settings;
        private readonly ILogger _logger;

        public RunScheduledJobs(AdvancedSettings settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public Task ExecuteAsync(IEnvelopePersistence storage, IDurabilityAgent agent)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return ExecuteAtTimeAsync(storage, agent, utcNow);
        }

        public async Task ExecuteAtTimeAsync(IEnvelopePersistence storage, IDurabilityAgent agent,
            DateTimeOffset utcNow)
        {
            var hasLock = await storage.Session.TryGetGlobalLock(TransportConstants.ScheduledJobLockId);
            if (!hasLock) return;

            await storage.Session.BeginAsync();

            try
            {
#pragma warning disable CS8600
                IReadOnlyList<Envelope> readyToExecute = null;
#pragma warning restore CS8600

                try
                {
                    readyToExecute = await storage.LoadScheduledToExecuteAsync(utcNow);

                    if (!readyToExecute.Any())
                    {
                        await storage.Session.RollbackAsync();
                        return;
                    }

                    await storage.ReassignIncomingAsync(_settings.UniqueNodeId, readyToExecute);

                    await storage.Session.CommitAsync();
                }
                catch (Exception)
                {
                    await storage.Session.RollbackAsync();
                    throw;
                }

                _logger.ScheduledJobsQueuedForExecution(readyToExecute);

                foreach (var envelope in readyToExecute)
                {
                    await agent.EnqueueLocallyAsync(envelope);
                }
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.ScheduledJobLockId);
            }
        }

        public string Description { get; } = "Run Scheduled Messages";
    }
}
