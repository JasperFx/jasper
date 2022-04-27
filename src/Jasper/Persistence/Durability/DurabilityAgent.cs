using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Jasper.Transports;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Durability
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
            return host.Services.GetRequiredService<IJasperRuntime>().As<JasperRuntime>().Durability;
        }

        private readonly IMessagingAction? IncomingMessages;
        private readonly IMessagingAction? OutgoingMessages;
        private readonly IMessagingAction? ScheduledJobs;
        private readonly IMessagingAction? NodeReassignment;

        private readonly IEnvelopePersistence? _storage;
        private readonly AdvancedSettings? _settings;
        private readonly ILogger<DurabilityAgent>? _trace;
        private readonly IWorkerQueue? _workers;

        private readonly ActionBlock<IMessagingAction?>? _worker;

        private Timer? _nodeReassignmentTimer;
        private Timer? _scheduledJobTimer;
        private readonly bool _disabled;
        private bool _hasStarted;


        public DurabilityAgent(IJasperRuntime runtime, ILogger logger,
            ILogger<DurabilityAgent> trace,
            IWorkerQueue? workers,
            IEnvelopePersistence? storage,
            AdvancedSettings? settings)
        {
            if (storage is NulloEnvelopePersistence)
            {
                _disabled = true;
                return;
            }

            Logger = logger;
            _trace = trace;
            _workers = workers;
            _storage = storage;
            _settings = settings;


            _worker = new ActionBlock<IMessagingAction?>(processAction, new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = 1,
                CancellationToken = _settings.Cancellation
            });

            NodeId = _settings.UniqueNodeId;

            IncomingMessages = new RecoverIncomingMessages(storage, workers, settings, logger);
            OutgoingMessages = new RecoverOutgoingMessages(runtime, settings, logger);
            NodeReassignment = new NodeReassignment(settings);
            ScheduledJobs = new RunScheduledJobs(settings, logger);
        }

        public ILogger Logger { get; }

        public int NodeId { get; }

        // This was built mostly for testing
        public Task Execute(IMessagingAction action)
        {
            // this is a side effect of the agent being shut down
            if (action == null) return Task.CompletedTask;

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

            public async Task Execute(IEnvelopePersistence? storage, IDurabilityAgent agent)
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

        public Task EnqueueLocally(Envelope? envelope)
        {
            return _workers.EnqueueAsync(envelope);
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

            await tryRestartConnectionAsync();

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

            await tryRestartConnectionAsync();

            // Something is wrong, but we'll keep trying in a bit
            if (!_storage.Session.IsConnected()) return;

            try
            {
                try
                {

                    if (_settings.VerboseDurabilityAgentLogging)
                    {
                        _trace.LogDebug("Running " + action.Description);
                    }
                    await action.Execute(_storage, this);
                }
                catch (Exception? e)
                {
                    Logger.LogError(e, "Running " + action.Description);
                }
            }
            catch (Exception? e)
            {
                Logger.LogError(e, "Error trying to run " + action);
                await _storage.Session.ReleaseNodeLock(_settings.UniqueNodeId);
            }

            await _storage.Session.GetNodeLock(_settings.UniqueNodeId);
        }

        private async Task tryRestartConnectionAsync()
        {
            if (_storage.Session.IsConnected()) return;

            try
            {
                await _storage.Session.ConnectAndLockCurrentNodeAsync(Logger, _settings.UniqueNodeId);
            }
            catch (Exception? e)
            {
                Logger.LogError(e, message: "Failure trying to restart the connection in DurabilityAgent");
            }
        }


        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_disabled) return;

            await _nodeReassignmentTimer.DisposeAsync();
            await _scheduledJobTimer.DisposeAsync();

            _worker.Complete();

            try
            {
                await _worker.Completion;

                await _storage.Session.ReleaseNodeLock(_settings.UniqueNodeId);

                // Release all envelopes tagged to this node in message persistence to any node
                await _storage.ReassignDormantNodeToAnyNodeAsync(_settings.UniqueNodeId);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Error while trying to stop DurabilityAgent");
            }
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
