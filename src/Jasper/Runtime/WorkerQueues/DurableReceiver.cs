using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime.WorkerQueues;

internal class DurableReceiver : ILocalQueue, IChannelCallback, ISupportNativeScheduling, ISupportDeadLetterQueue, IAsyncDisposable
{
    private readonly ILogger _logger;
    private readonly IEnvelopePersistence _persistence;
    private readonly ActionBlock<Envelope> _receiver;
    private readonly AdvancedSettings _settings;

    public DurableReceiver(Endpoint endpoint, IJasperRuntime runtime)
    {
        _settings = runtime.Advanced;
        _persistence = runtime.Persistence;
        _logger = runtime.Logger;
        var pipeline = runtime.Pipeline;

        endpoint.ExecutionOptions.CancellationToken = _settings.Cancellation;

        _receiver = new ActionBlock<Envelope>(async envelope =>
        {
            try
            {
                envelope.ContentType ??= EnvelopeConstants.JsonContentType;

                await pipeline.InvokeAsync(envelope, this);
            }
            catch (Exception? e)
            {
                // TODO -- how does this get recovered?

                // This *should* never happen, but of course it will
                _logger.LogError(e, "Unexpected pipeline invocation error");
            }
        }, endpoint.ExecutionOptions);
    }

    private async Task executeWithRetriesAsync(Func<Task> action)
    {
        var i = 0;
        while (true)
        {
            try
            {
                await action().ConfigureAwait(false);
                return;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected failure");
                i++;
                await Task.Delay(i * 100).ConfigureAwait(false);
            }
        }
    }

    public Uri? Address { get; set; }

    public async ValueTask CompleteAsync(Envelope envelope)
    {
        await executeWithRetriesAsync(() => _persistence.DeleteIncomingEnvelopeAsync(envelope));
    }

    public async ValueTask DeferAsync(Envelope envelope)
    {
        envelope.Attempts++;

        Enqueue(envelope);

        await executeWithRetriesAsync(() => _persistence.IncrementIncomingEnvelopeAttemptsAsync(envelope));
    }

    public Task MoveToErrorsAsync(Envelope envelope, Exception exception)
    {
        var errorReport = new ErrorReport(envelope, exception);

        return executeWithRetriesAsync(() => _persistence.MoveToDeadLetterStorageAsync(new[] { errorReport }));
    }

    public Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
    {
        envelope.OwnerId = TransportConstants.AnyNode;
        envelope.ScheduledTime = time;
        envelope.Status = EnvelopeStatus.Scheduled;

        return executeWithRetriesAsync(() => _persistence.ScheduleExecutionAsync(new[] { envelope }));
    }

    public void Enqueue(Envelope envelope)
    {
        envelope.ReplyUri = envelope.ReplyUri ?? Address;
        _receiver.Post(envelope);
    }

    [Obsolete]
    public void StartListening(IListener listener)
    {
        listener.Start(this, _settings.Cancellation);

        Address = listener.Address;
    }


    public ValueTask ReceivedAsync(IListener listener, Envelope[] messages)
    {
        var now = DateTimeOffset.Now;

        return ProcessReceivedMessagesAsync(now, listener, messages);
    }

    public async ValueTask ReceivedAsync(IListener listener, Envelope envelope)
    {
        if (listener == null)
        {
            throw new ArgumentNullException(nameof(listener));
        }

        if (envelope == null)
        {
            throw new ArgumentNullException(nameof(envelope));
        }

        using var activity = JasperTracing.StartExecution(_settings.OpenTelemetryReceiveSpanName!, envelope,
            ActivityKind.Consumer);
        var now = DateTimeOffset.Now;
        envelope.MarkReceived(listener, now, _settings);

        await _persistence.StoreIncomingAsync(envelope);

        if (envelope.Status == EnvelopeStatus.Incoming)
        {
            Enqueue(envelope);
        }

        await listener.CompleteAsync(envelope);

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
    public async ValueTask ProcessReceivedMessagesAsync(DateTimeOffset now, IListener listener, Envelope[] envelopes)
    {
        if (_settings.Cancellation.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        foreach (var envelope in envelopes)
        {
            envelope.MarkReceived(listener, now, _settings);
        }

        await _persistence.StoreIncomingAsync(envelopes);

        foreach (var message in envelopes)
        {
            Enqueue(message);
            await listener.CompleteAsync(message);
        }

        _logger.IncomingBatchReceived(envelopes);
    }
}
