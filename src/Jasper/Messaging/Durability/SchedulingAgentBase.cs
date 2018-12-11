using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Durability
{
    public abstract class SchedulingAgentBase<T> : IHostedService, IDisposable, ISchedulingAgent
    {
        private readonly ActionBlock<T> _worker;
        private Timer _nodeReassignmentTimer;
        private Timer _scheduledJobTimer;

        protected SchedulingAgentBase(JasperOptions settings, ITransportLogger logger, T scheduledJobs,
            T incomingMessages, T outgoingMessages, T nodeReassignment)
        {
            ScheduledJobs = scheduledJobs;
            IncomingMessages = incomingMessages;
            OutgoingMessages = outgoingMessages;
            NodeReassignment = nodeReassignment;


            this.settings = settings;
            this.logger = logger;

            _worker = new ActionBlock<T>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1
            });
        }

        protected JasperOptions settings { get; }

        protected ITransportLogger logger { get; }

        public T ScheduledJobs { get; }

        public T IncomingMessages { get; }

        public T OutgoingMessages { get; }

        public T NodeReassignment { get; }


        public void Dispose()
        {
            disposeConnection();
            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await openConnectionAndAttainNodeLock();

            _scheduledJobTimer = new Timer(s =>
            {
                _worker.Post(ScheduledJobs);
                _worker.Post(IncomingMessages);
                _worker.Post(OutgoingMessages);
            }, settings, settings.ScheduledJobs.FirstExecution, settings.ScheduledJobs.PollingTime);

            _nodeReassignmentTimer = new Timer(s => { _worker.Post(NodeReassignment); }, settings,
                settings.Retries.FirstNodeReassignmentExecution, settings.Retries.NodeReassignmentPollingTime);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _worker.Complete();

            await _worker.Completion;

            await releaseNodeLockAndClose();
        }


        public void RescheduleOutgoingRecovery()
        {
            _worker.Post(OutgoingMessages);
        }

        public void RescheduleIncomingRecovery()
        {
            _worker.Post(IncomingMessages);
        }

        protected abstract void disposeConnection();
        protected abstract Task processAction(T action);
        protected abstract Task openConnectionAndAttainNodeLock();
        protected abstract Task releaseNodeLockAndClose();
    }
}
