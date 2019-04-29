using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.WorkerQueues;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Durability
{


    public class DurabilityAgent : IHostedService, IDurabilityAgent, IDisposable
    {
        public static DurabilityAgent ForHost(IJasperHost host)
        {
            return host.Get<JasperOptions>().DurabilityAgent;
        }

        public static DurabilityAgent ForHost(IWebHost host)
        {
            return host.Services.GetRequiredService<JasperOptions>().DurabilityAgent;
        }

        private readonly IMessagingAction IncomingMessages;
        private readonly IMessagingAction OutgoingMessages;
        private readonly IMessagingAction ScheduledJobs;
        private readonly IMessagingAction NodeReassignment;

        private readonly JasperOptions _options;
        private readonly IEnvelopePersistence _persistence;
        private readonly IDurabilityAgentStorage _storage;
        private readonly ILogger<DurabilityAgent> _trace;
        private readonly IWorkerQueue _workers;

        private readonly ActionBlock<IMessagingAction> _worker;

        private Timer _nodeReassignmentTimer;
        private Timer _scheduledJobTimer;
        private readonly bool _disabled;
        private bool _hasStarted;


        public DurabilityAgent(JasperOptions options,
            ITransportLogger logger,
            ILogger<DurabilityAgent> trace,
            IWorkerQueue workers,
            IEnvelopePersistence persistence,
            ISubscriberGraph subscribers)
        {
            if (persistence is NulloEnvelopePersistence)
            {
                _disabled = true;
                return;
            }

            options.DurabilityAgent = this;

            _options = options;
            Logger = logger;
            _trace = trace;
            _workers = workers;
            _persistence = persistence;



            _storage = _persistence.AgentStorage;

            _worker = new ActionBlock<IMessagingAction>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _options.Cancellation
            });

            NodeId = _options.UniqueNodeId;

            IncomingMessages = new RecoverIncomingMessages(workers, options, logger);
            OutgoingMessages = new RecoverOutgoingMessages(subscribers, options, logger);
            NodeReassignment = new NodeReassignment(options, logger);
            ScheduledJobs = new RunScheduledJobs(options, logger);
        }

        public ITransportLogger Logger { get; }

        public int NodeId { get; }

        // This was built mostly for testing
        public Task Execute(IMessagingAction action)
        {
            if (_hasStarted)
            {
                var wrapper = new MessageActionWrapper(action);
                _worker.Post(wrapper);

                return wrapper.Completion;
            }
            else
            {
                return processAction(action);
            }
        }

        public class MessageActionWrapper : IMessagingAction
        {
            private readonly IMessagingAction _inner;
            private readonly TaskCompletionSource<bool> _completion;

            public MessageActionWrapper(IMessagingAction inner)
            {
                _inner = inner;
                _completion = new TaskCompletionSource<bool>();
            }

            public string Description => _inner.Description;

            public Task Completion => _completion.Task;

            public async Task Execute(IDurabilityAgentStorage storage, IDurabilityAgent agent)
            {
                try
                {
                    await _inner.Execute(storage, agent);
                    _completion.SetResult(true);
                }
                catch (Exception e)
                {
                    _completion.SetException(e);
                }
            }
        }

        public Task EnqueueLocally(Envelope envelope)
        {
            envelope.Callback = new DurableCallback(envelope, _workers, _persistence, Logger);

            return _workers.Enqueue(envelope);
        }

        public void RescheduleIncomingRecovery()
        {
            _worker.Post(IncomingMessages);
        }

        public void RescheduleOutgoingRecovery()
        {
            _worker.Post(OutgoingMessages);
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            if (_disabled) return;



            _hasStarted = true;

            await tryRestartConnection();

            _scheduledJobTimer = new Timer(s =>
            {
                _worker.Post(ScheduledJobs);
                _worker.Post(IncomingMessages);
                _worker.Post(OutgoingMessages);
            }, _options, _options.ScheduledJobs.FirstExecution, _options.ScheduledJobs.PollingTime);

            _nodeReassignmentTimer = new Timer(s => { _worker.Post(NodeReassignment); }, _options,
                _options.Retries.FirstNodeReassignmentExecution, _options.Retries.NodeReassignmentPollingTime);
        }

        private async Task processAction(IMessagingAction action)
        {
            if (_options.Cancellation.IsCancellationRequested) return;

            await tryRestartConnection();

            // Something is wrong, but we'll keep trying in a bit
            if (!_storage.Session.IsConnected()) return;

            try
            {
                try
                {
                    _trace.LogDebug("Running " + action.Description);
                    await action.Execute(_storage, this);
                }
                catch (Exception e)
                {
                    Logger.LogException(e, message: "Running " + action.Description);
                }
            }
            catch (Exception e)
            {
                Logger.LogException(e, message: "Error trying to run " + action);
                await _storage.Session.ReleaseNodeLock(_options.UniqueNodeId);
            }

            await _storage.Session.GetNodeLock(_options.UniqueNodeId);
        }

        private async Task tryRestartConnection()
        {
            if (_storage.Session.IsConnected()) return;

            try
            {
                await _storage.Session.ConnectAndLockCurrentNode(Logger, _options.UniqueNodeId);
            }
            catch (Exception e)
            {
                Logger.LogException(e, message:"Failure trying to restart the connection in DurabilityAgent");
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_disabled) return;

            _worker.Complete();

            await _worker.Completion;

            await _storage.Session.ReleaseNodeLock(_options.UniqueNodeId);

            // Release all envelopes tagged to this node in message persistence to any node
            await _storage.Nodes.ReassignDormantNodeToAnyNode(_options.UniqueNodeId);

            _storage.Dispose();
        }

        public void Dispose()
        {
            if (_disabled) return;

            if (_storage.Session.IsConnected())
            {
                _storage.Session.ReleaseNodeLock(_options.UniqueNodeId).GetAwaiter().GetResult();
                _storage.SafeDispose();
            }

            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }
    }
}
