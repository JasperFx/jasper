using System;
using System.Linq;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging.Durability
{
    internal class RunScheduledJobs : IMessagingAction
    {
        private readonly JasperOptions _options;
        private readonly ITransportLogger _logger;

        public RunScheduledJobs(JasperOptions options, ITransportLogger logger)
        {
            _options = options;
            _logger = logger;
        }

        public Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
        {
            var utcNow = DateTimeOffset.UtcNow;
            return ExecuteAtTime(storage, agent, utcNow);
        }

        public async Task<Envelope[]> ExecuteAtTime(IDurabilityAgentStorage storage, IDurabilityAgent agent, DateTimeOffset utcNow)
        {
            var hasLock = await storage.Session.TryGetGlobalLock(TransportConstants.ScheduledJobLockId);
            if (!hasLock) return null;

            await storage.Session.Begin();

            try
            {
                Envelope[] readyToExecute = null;

                try
                {
                    // TODO -- this needs to be paged to keep it from being too big
                    readyToExecute = await storage.LoadScheduledToExecute(utcNow);

                    if (!readyToExecute.Any())
                    {
                        await storage.Session.Rollback();
                        return readyToExecute;
                    }

                    await storage.Incoming.Reassign(_options.UniqueNodeId, readyToExecute);

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
