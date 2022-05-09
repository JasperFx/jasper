using System;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Runtime;
using Jasper.Runtime.WorkerQueues;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Durability;

public class DurabilityAgent : IHostedService, IDurabilityAgent, IAsyncDisposable
{
    private readonly bool _disabled;
    private readonly IMessagingAction _incomingMessages;
    private readonly ILogger _logger;
    private readonly IMessagingAction _nodeReassignment;
    private readonly IMessagingAction _outgoingMessages;
    private readonly IMessagingAction _scheduledJobs;
    private readonly AdvancedSettings _settings;

    private readonly IEnvelopePersistence _storage;
    private readonly ILogger<DurabilityAgent> _trace;

    private readonly ActionBlock<IMessagingAction> _worker;
    private readonly IWorkerQueue _workers;

    private Timer? _nodeReassignmentTimer;
    private Timer? _scheduledJobTimer;

#pragma warning disable CS8618
    public DurabilityAgent(IJasperRuntime runtime, ILogger logger,
#pragma warning restore CS8618
        ILogger<DurabilityAgent> trace,
        IWorkerQueue workers,
        IEnvelopePersistence storage,
        AdvancedSettings settings)
    {
        if (storage is NullEnvelopePersistence)
        {
            _disabled = true;
            return;
        }

        _logger = logger;
        _trace = trace;
        _workers = workers;
        _storage = storage;
        _settings = settings;


        _worker = new ActionBlock<IMessagingAction>(processActionAsync, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1,
            CancellationToken = _settings.Cancellation
        });

        _incomingMessages = new RecoverIncomingMessages(workers, settings, logger);
        _outgoingMessages = new RecoverOutgoingMessages(runtime, settings, logger);
        _nodeReassignment = new NodeReassignment(settings);
        _scheduledJobs = new RunScheduledJobs(settings, logger);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disabled)
        {
            return;
        }

        if (_storage.Session.IsConnected())
        {
            await _storage.Session.ReleaseNodeLockAsync(_settings.UniqueNodeId);
            _storage.SafeDispose();
        }

        if (_scheduledJobTimer != null)
        {
            await _scheduledJobTimer.DisposeAsync();
        }

        if (_nodeReassignmentTimer != null)
        {
            await _nodeReassignmentTimer.DisposeAsync();
        }
    }

    public Task EnqueueLocallyAsync(Envelope envelope)
    {
        return _workers.EnqueueAsync(envelope);
    }

    public void RescheduleIncomingRecovery()
    {
        _worker.Post(_incomingMessages);
    }

    public void RescheduleOutgoingRecovery()
    {
        _worker.Post(_outgoingMessages);
    }

    public async Task StartAsync(CancellationToken stoppingToken)
    {
        if (_disabled)
        {
            return;
        }

        await tryRestartConnectionAsync();

        _scheduledJobTimer = new Timer(_ =>
        {
            _worker.Post(_scheduledJobs);
            _worker.Post(_incomingMessages);
            _worker.Post(_outgoingMessages);
        }, _settings, _settings.ScheduledJobFirstExecution, _settings.ScheduledJobPollingTime);

        _nodeReassignmentTimer = new Timer(_ => _worker.Post(_nodeReassignment), _settings,
            _settings.FirstNodeReassignmentExecution, _settings.NodeReassignmentPollingTime);
    }


    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_disabled)
        {
            return;
        }

        if (_nodeReassignmentTimer != null)
        {
            await _nodeReassignmentTimer.DisposeAsync();
        }

        if (_scheduledJobTimer != null)
        {
            await _scheduledJobTimer.DisposeAsync();
        }

        _worker.Complete();

        try
        {
            await _worker.Completion;

            await _storage.Session.ReleaseNodeLockAsync(_settings.UniqueNodeId);

            // Release all envelopes tagged to this node in message persistence to any node
            await _storage.ReassignDormantNodeToAnyNodeAsync(_settings.UniqueNodeId);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while trying to stop DurabilityAgent");
        }
    }

    private async Task processActionAsync(IMessagingAction action)
    {
        if (_settings.Cancellation.IsCancellationRequested)
        {
            return;
        }

        await tryRestartConnectionAsync();

        // Something is wrong, but we'll keep trying in a bit
        if (!_storage.Session.IsConnected())
        {
            return;
        }

        try
        {
            try
            {
                if (_settings.VerboseDurabilityAgentLogging)
                {
                    _trace.LogDebug("Running action {Action}", action.Description);
                }

                await action.ExecuteAsync(_storage, this);
            }
            catch (Exception? e)
            {
                _logger.LogError(e, "Error while running {Action}", action.Description);
            }
        }
        catch (Exception? e)
        {
            _logger.LogError(e, "Error trying to run {Action}", action);
            await _storage.Session.ReleaseNodeLockAsync(_settings.UniqueNodeId);
        }

        await _storage.Session.GetNodeLockAsync(_settings.UniqueNodeId);
    }

    private async Task tryRestartConnectionAsync()
    {
        if (_storage.Session.IsConnected())
        {
            return;
        }

        try
        {
            await _storage.Session.ConnectAndLockCurrentNodeAsync(_logger, _settings.UniqueNodeId);
        }
        catch (Exception? e)
        {
            _logger.LogError(e, "Failure trying to restart the connection in DurabilityAgent");
        }
    }
}
