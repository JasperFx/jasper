using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.WorkerQueues;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Messaging.Durability
{


    public class DurabilityAgent : IHostedService, IDurabilityAgent, IDisposable
    {
        /// <summary>
        /// Strictly a testing helper
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static DurabilityAgent ForHost(IHost host)
        {
            return host.Services.GetService<IMessagingRoot>().As<MessagingRoot>().Durability;
        }

        private readonly IMessagingAction IncomingMessages;
        private readonly IMessagingAction OutgoingMessages;
        private readonly IMessagingAction ScheduledJobs;
        private readonly IMessagingAction NodeReassignment;

        private readonly IEnvelopePersistence _persistence;
        private readonly AdvancedSettings _settings;
        private readonly IDurabilityAgentStorage _storage;
        private readonly ILogger<DurabilityAgent> _trace;
        private readonly IWorkerQueue _workers;

        private readonly ActionBlock<IMessagingAction> _worker;

        private Timer _nodeReassignmentTimer;
        private Timer _scheduledJobTimer;
        private readonly bool _disabled;
        private bool _hasStarted;


        public DurabilityAgent(ITransportLogger logger,
            ILogger<DurabilityAgent> trace,
            IWorkerQueue workers,
            IEnvelopePersistence persistence,
            ITransportRuntime runtime,
            AdvancedSettings settings)
        {
            if (persistence is NulloEnvelopePersistence)
            {
                _disabled = true;
                return;
            }

            Logger = logger;
            _trace = trace;
            _workers = workers;
            _persistence = persistence;
            _settings = settings;


            _storage = _persistence.AgentStorage;

            _worker = new ActionBlock<IMessagingAction>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _settings.Cancellation
            });

            NodeId = _settings.UniqueNodeId;

            IncomingMessages = new RecoverIncomingMessages(persistence, workers, settings, logger);
            OutgoingMessages = new RecoverOutgoingMessages(runtime, settings, logger);
            NodeReassignment = new NodeReassignment(settings);
            ScheduledJobs = new RunScheduledJobs(settings, logger);
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
            }, _settings, _settings.ScheduledJobFirstExecution, _settings.ScheduledJobPollingTime);

            _nodeReassignmentTimer = new Timer(s => { _worker.Post(NodeReassignment); }, _settings,
                _settings.FirstNodeReassignmentExecution, _settings.NodeReassignmentPollingTime);
        }

        private async Task processAction(IMessagingAction action)
        {
            if (_settings.Cancellation.IsCancellationRequested) return;

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
                await _storage.Session.ReleaseNodeLock(_settings.UniqueNodeId);
            }

            await _storage.Session.GetNodeLock(_settings.UniqueNodeId);
        }

        private async Task tryRestartConnection()
        {
            if (_storage.Session.IsConnected()) return;

            try
            {
                await _storage.Session.ConnectAndLockCurrentNode(Logger, _settings.UniqueNodeId);
            }
            catch (Exception e)
            {
                Logger.LogException(e, message:"Failure trying to restart the connection in DurabilityAgent");
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_disabled) return;

            await _nodeReassignmentTimer.DisposeAsync();
            await _scheduledJobTimer.DisposeAsync();

            _worker.Complete();

            await _worker.Completion;

            await _storage.Session.ReleaseNodeLock(_settings.UniqueNodeId);

            // Release all envelopes tagged to this node in message persistence to any node
            await _storage.Nodes.ReassignDormantNodeToAnyNode(_settings.UniqueNodeId);

            _storage.Dispose();
        }

        public void Dispose()
        {
            if (_disabled) return;

            if (_storage.Session.IsConnected())
            {
                _storage.Session.ReleaseNodeLock(_settings.UniqueNodeId).GetAwaiter().GetResult();
                _storage.SafeDispose();
            }

            _scheduledJobTimer?.Dispose();
            _nodeReassignmentTimer?.Dispose();
        }
    }
}
