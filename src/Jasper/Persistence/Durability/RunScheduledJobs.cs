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
        private readonly AdvancedSettings? _settings;
        private readonly ILogger _logger;

        public RunScheduledJobs(AdvancedSettings? settings, ILogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public Task Execute(IEnvelopePersistence? storage, IDurabilityAgent agent)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return ExecuteAtTime(storage, agent, utcNow);
        }

        public async Task<IReadOnlyList<Envelope?>> ExecuteAtTime(IEnvelopePersistence? storage, IDurabilityAgent agent, DateTimeOffset utcNow)
        {
            var hasLock = await storage.Session.TryGetGlobalLock(TransportConstants.ScheduledJobLockId);
            if (!hasLock) return null;

            await storage.Session.Begin();

            try
            {
                IReadOnlyList<Envelope?> readyToExecute = null;

                try
                {
                    readyToExecute = await storage.LoadScheduledToExecuteAsync(utcNow);

                    if (!readyToExecute.Any())
                    {
                        await storage.Session.Rollback();
                        return readyToExecute;
                    }

                    await storage.ReassignIncomingAsync(_settings.UniqueNodeId, readyToExecute);

                    await storage.Session.Commit();
                }
                catch (Exception)
                {
                    await storage.Session.Rollback();
                    throw;
                }

                _logger.ScheduledJobsQueuedForExecution(readyToExecute);

                foreach (var envelope in readyToExecute)
                {
                    await agent.EnqueueLocally(envelope);


                }

                return readyToExecute;
            }
            finally
            {
                await storage.Session.ReleaseGlobalLock(TransportConstants.ScheduledJobLockId);
            }
        }

        public string Description { get; } = "Run Scheduled Messages";
    }
}
