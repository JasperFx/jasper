using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline.Dates;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Jasper.Runtime.WorkerQueues;

public class DurableWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling, IHasDeadLetterQueue, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly IEnvelopePersistence _persistence;
    private readonly AsyncRetryPolicy _policy;
    private readonly ActionBlock<Envelope> _receiver;
    private readonly AdvancedSettings _settings;
    private IListener? _listener;

    public DurableWorkerQueue(Endpoint endpoint, IHandlerPipeline pipeline,
        AdvancedSettings settings, IEnvelopePersistence persistence, ILogger logger)
    {
        _settings = settings;
        _persistence = persistence;
        _logger = logger;

        endpoint.ExecutionOptions.CancellationToken = settings.Cancellation;

        _receiver = new ActionBlock<Envelope>(async envelope =>
        {
            try
            {
                envelope.ContentType ??= EnvelopeConstants.JsonContentType;

                await pipeline.InvokeAsync(envelope, this);
            }
            catch (Exception? e)
            {
                // This *should* never happen, but of course it will
                logger.LogError(e, "Unexpected pipeline invocation error");
            }
        }, endpoint.ExecutionOptions);

        _policy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(i => (i * 100).Milliseconds()
                , (e, _) => _logger.LogError(e, "Unexpected failure"));
    }

    public Uri? Address { get; set; }

    public async ValueTask CompleteAsync(Envelope envelope)
    {
        await _policy.ExecuteAsync(() => _persistence.DeleteIncomingEnvelopeAsync(envelope));
    }

    public async ValueTask DeferAsync(Envelope envelope)
    {
        envelope.Attempts++;

        await EnqueueAsync(envelope);

        await _policy.ExecuteAsync(() => _persistence.IncrementIncomingEnvelopeAttemptsAsync(envelope));
    }

    public Task MoveToErrorsAsync(Envelope envelope, Exception exception)
    {
        var errorReport = new ErrorReport(envelope, exception);

        return _policy.ExecuteAsync(() => _persistence.MoveToDeadLetterStorageAsync(new[] { errorReport }));
    }

    public Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
    {
        envelope.OwnerId = TransportConstants.AnyNode;
        envelope.ScheduledTime = time;
        envelope.Status = EnvelopeStatus.Scheduled;

        return _policy.ExecuteAsync(() => _persistence.ScheduleExecutionAsync(new[] { envelope }));
    }

    public int QueuedCount => _receiver.InputCount;

    public Task EnqueueAsync(Envelope envelope)
    {
        envelope.ReplyUri = envelope.ReplyUri ?? Address;
        _receiver.Post(envelope);

        return Task.CompletedTask;
    }

    public Task ScheduleExecutionAsync(Envelope envelope)
    {
        return Task.CompletedTask;
    }

    public void StartListening(IListener listener)
    {
        _listener = listener;
        _listener.Start(this, _settings.Cancellation);

        Address = _listener.Address;
    }


    public Task ReceivedAsync(Uri uri, Envelope[] messages)
    {
        var now = DateTime.UtcNow;

        return ProcessReceivedMessagesAsync(now, uri, messages);
    }

    public async Task ReceivedAsync(Uri uri, Envelope envelope)
    {
        if (_listener == null) throw new InvalidOperationException($"Worker queue for {uri} has not been started");

        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryReceiveSpanName!, envelope,
            ActivityKind.Consumer);
        var now = DateTime.UtcNow;
        envelope.MarkReceived(uri, now, _settings.UniqueNodeId);

        await _persistence.StoreIncomingAsync(envelope);

        if (envelope.Status == EnvelopeStatus.Incoming)
        {
            await EnqueueAsync(envelope);
        }

        await _listener.CompleteAsync(envelope);

        _logger.IncomingReceived(envelope, Address);
    }


    public void Dispose()
    {
        // Might need to drain the block
        _receiver.Complete();
    }

    public async ValueTask DisposeAsync()
    {
        _receiver.Complete();
        await _receiver.Completion;
    }

    // Separated for testing here.
    public async Task ProcessReceivedMessagesAsync(DateTime now, Uri uri, Envelope[] envelopes)
    {
        if (_settings.Cancellation.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        foreach (var envelope in envelopes) envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);

        await _persistence.StoreIncomingAsync(envelopes);

        foreach (var message in envelopes)
        {
            await EnqueueAsync(message);
            await _listener!.CompleteAsync(message);
        }

        _logger.IncomingBatchReceived(envelopes);
    }
}
