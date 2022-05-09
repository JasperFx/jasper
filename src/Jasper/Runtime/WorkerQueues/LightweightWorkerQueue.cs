using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Baseline;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime.Scheduled;
using Jasper.Transports;
using Microsoft.Extensions.Logging;

namespace Jasper.Runtime.WorkerQueues;

public class LightweightWorkerQueue : IWorkerQueue, IChannelCallback, IHasNativeScheduling
{
    private readonly ILogger _logger;
    private readonly ActionBlock<Envelope> _receiver;
    private readonly InMemoryScheduledJobProcessor _scheduler;
    private readonly AdvancedSettings _settings;
    private IListener? _listener;

    public LightweightWorkerQueue(Endpoint endpoint, ILogger logger,
        IHandlerPipeline pipeline, AdvancedSettings settings)
    {
        _logger = logger;
        _settings = settings;
        Pipeline = pipeline;

        _scheduler = new InMemoryScheduledJobProcessor(this);

        endpoint.ExecutionOptions.CancellationToken = settings.Cancellation;

        _receiver = new ActionBlock<Envelope>(async envelope =>
        {
            try
            {
                if (envelope.ContentType.IsEmpty())
                {
                    envelope.ContentType = EnvelopeConstants.JsonContentType;
                }

                await Pipeline.InvokeAsync(envelope, this);
            }
            catch (Exception? e)
            {
                // This *should* never happen, but of course it will
                logger.LogError(e, "Unexpected error in Pipeline invocation");
            }
        }, endpoint.ExecutionOptions);
    }

    public IHandlerPipeline Pipeline { get; }

    public Uri? Address { get; set; }

    ValueTask IChannelCallback.CompleteAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    async ValueTask IChannelCallback.DeferAsync(Envelope envelope)
    {
        if (_listener == null)
        {
            Enqueue(envelope);
            return;
        }

        var nativelyRequeued = await _listener.TryRequeueAsync(envelope);
        if (!nativelyRequeued)
        {
            Enqueue(envelope);
        }
    }

    Task IHasNativeScheduling.MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
    {
        envelope.ScheduledTime = time;
        ScheduleExecution(envelope);

        return Task.CompletedTask;
    }

    public int QueuedCount => _receiver.InputCount;

    public void Enqueue(Envelope envelope)
    {
        if (envelope.IsPing())
        {
            return;
        }

        _receiver.Post(envelope);
    }

    public void ScheduleExecution(Envelope envelope)
    {
        if (!envelope.ScheduledTime.HasValue)
        {
            throw new ArgumentOutOfRangeException(nameof(envelope),
                $"There is no {nameof(Envelope.ScheduledTime)} value");
        }

        _scheduler.Enqueue(envelope.ScheduledTime.Value, envelope);
    }


    public void StartListening(IListener listener)
    {
        _listener = listener;
        _listener.Start(this, _settings.Cancellation);

        Address = _listener.Address;
    }

    Task IListeningWorkerQueue.ReceivedAsync(Uri uri, Envelope[] messages)
    {
        var now = DateTime.UtcNow;

        return ProcessReceivedMessagesAsync(now, uri, messages);
    }

    public async ValueTask ReceivedAsync(Uri uri, Envelope envelope)
    {
        if (_listener == null)
        {
            throw new InvalidOperationException("This worker queue has not been initialized with a listener");
        }

        var now = DateTime.UtcNow;
        envelope.MarkReceived(uri, now, _settings.UniqueNodeId);

        if (envelope.IsExpired())
        {
            return;
        }

        if (envelope.Status == EnvelopeStatus.Scheduled)
        {
            _scheduler.Enqueue(envelope.ScheduledTime!.Value, envelope);
        }
        else
        {
            Enqueue(envelope);
        }

        await _listener.CompleteAsync(envelope);

        _logger.IncomingReceived(envelope, Address);
    }

    public void Dispose()
    {
        _receiver.Complete();
    }

    // Separated for testing here.
    public async Task ProcessReceivedMessagesAsync(DateTime now, Uri uri, Envelope[] envelopes)
    {
        if (_settings.Cancellation.IsCancellationRequested)
        {
            throw new OperationCanceledException();
        }

        if (_listener == null)
        {
            throw new InvalidOperationException("This worker queue has not been initialized with a listener");
        }

        foreach (var envelope in envelopes)
        {
            envelope.MarkReceived(uri, DateTime.UtcNow, _settings.UniqueNodeId);
            Enqueue(envelope);
            await _listener.CompleteAsync(envelope);
        }

        _logger.IncomingBatchReceived(envelopes);
    }
}
