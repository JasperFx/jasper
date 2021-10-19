using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Transports;

namespace Jasper.Persistence.Durability
{
    internal class RunScheduledJobs : IMessagingAction
    {
        private readonly AdvancedSettings _settings;
        private readonly ITransportLogger _logger;

        public RunScheduledJobs(AdvancedSettings settings, ITransportLogger logger)
        {
            _settings = settings;
            _logger = logger;
        }

        public Task Execute(IEnvelopePersistence storage, IDurabilityAgent agent)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return ExecuteAtTime(storage, agent, utcNow);
        }

        public async Task<Envelope[]> ExecuteAtTime(IEnvelopePersistence storage, IDurabilityAgent agent, DateTimeOffset utcNow)
        {
            var hasLock = await storage.Session.TryGetGlobalLock(TransportConstants.ScheduledJobLockId);
            if (!hasLock) return null;

            await storage.Session.Begin();

            try
            {
                Envelope[] readyToExecute = null;

                try
                {
                    readyToExecute = await storage.LoadScheduledToExecute(utcNow);

                    if (!readyToExecute.Any())
                    {
                        await storage.Session.Rollback();
                        return readyToExecute;
                    }

                    await storage.ReassignIncoming(_settings.UniqueNodeId, readyToExecute);

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
