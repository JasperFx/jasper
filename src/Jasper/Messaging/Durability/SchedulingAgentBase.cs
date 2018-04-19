using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Durability
{
    public abstract class SchedulingAgentBase<T>: IHostedService, IDisposable, ISchedulingAgent
    {
        private readonly ActionBlock<T> _worker;
        private Timer _scheduledJobTimer;
        private Timer _nodeReassignmentTimer;

        protected SchedulingAgentBase(MessagingSettings settings, ITransportLogger logger, T scheduledJobs, T incomingMessages, T outgoingMessages, T nodeReassignment)
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

        protected MessagingSettings settings { get; }

        protected ITransportLogger logger { get; }

        public T ScheduledJobs { get; }

        public T IncomingMessages { get; }

        public T OutgoingMessages { get; }

        public T NodeReassignment { get; }


        public void RescheduleOutgoingRecovery()
        {
            _worker.Post(OutgoingMessages);
        }

        public void RescheduleIncomingRecovery()
        {
            _worker.Post(IncomingMessages);
        }


        public void Dispose()
        {
            disposeConnection();
            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }

        protected abstract void disposeConnection();
        protected abstract Task processAction(T action);
        protected abstract Task openConnectionAndAttainNodeLock();
        protected abstract Task releaseNodeLockAndClose();

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await openConnectionAndAttainNodeLock();

            _scheduledJobTimer = new Timer(s =>
            {
                _worker.Post(ScheduledJobs);
                _worker.Post(IncomingMessages);
                _worker.Post(OutgoingMessages);

            }, settings, settings.FirstScheduledJobExecution, settings.ScheduledJobPollingTime);

            _nodeReassignmentTimer = new Timer(s =>
            {
                _worker.Post(NodeReassignment);


            }, settings, settings.FirstNodeReassignmentExecution, settings.NodeReassignmentPollingTime);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _worker.Complete();

            await _worker.Completion;

            await releaseNodeLockAndClose();
        }
    }
}
