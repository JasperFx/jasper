using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Persistence.Durability;
using Jasper.Transports;
using Jasper.Util;

namespace Jasper.Runtime;

public class ExecutionContext : MessagePublisher, IExecutionContext, IEnvelopeTransaction
{
    private readonly IList<Envelope> _scheduled = new List<Envelope>();
    private IChannelCallback? _channel;
    private object? _sagaId;

    public ExecutionContext(IJasperRuntime runtime) : base(runtime, Guid.NewGuid().ToString())
    {
    }

    Task IEnvelopeTransaction.PersistAsync(Envelope envelope)
    {
        _outstanding.Fill(envelope);
        return Task.CompletedTask;
    }

    Task IEnvelopeTransaction.PersistAsync(Envelope[] envelopes)
    {
        _outstanding.Fill(envelopes);
        return Task.CompletedTask;
    }

    Task IEnvelopeTransaction.ScheduleJobAsync(Envelope envelope)
    {
        _scheduled.Fill(envelope);
        return Task.CompletedTask;
    }

    async Task IEnvelopeTransaction.CopyToAsync(IEnvelopeTransaction other)
    {
        await other.PersistAsync(_outstanding.ToArray());

        foreach (var envelope in _scheduled) await other.ScheduleJobAsync(envelope);
    }

    /// <summary>
    ///     Send a response message back to the original sender of the message being handled.
    ///     This can only be used from within a message handler
    /// </summary>
    /// <param name="context"></param>
    /// <param name="response"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public Task RespondToSenderAsync(object? response)
    {
        if (Envelope == null)
        {
            throw new InvalidOperationException(
                "This operation can only be performed while in the middle of handling an incoming message");
        }

        if (Envelope.ReplyUri == null)
        {
            throw new ArgumentOutOfRangeException(nameof(Envelope), $"There is no {nameof(Envelope.ReplyUri)}");
        }

        return SendToDestinationAsync(Envelope.ReplyUri, response);
    }


    public async Task EnqueueCascadingAsync(object? message)
    {
        if (Envelope == null) throw new InvalidOperationException("No Envelope attached to this context");

        if (Envelope.ResponseType != null && (message?.GetType() == Envelope.ResponseType ||
                                              Envelope.ResponseType.IsAssignableFrom(message?.GetType())))
        {
            Envelope.Response = message;
            return;
        }

        switch (message)
        {
            case null:
                return;

            case Envelope env:
                await SendEnvelopeAsync(env);
                return;

            case IEnumerable<object> enumerable:
                foreach (var o in enumerable) await EnqueueCascadingAsync(o);

                return;
        }

        if (message.GetType().ToMessageTypeName() == Envelope.ReplyRequested)
        {
            await SendToDestinationAsync(Envelope.ReplyUri!, message);
            return;
        }


        await PublishAsync(message);
    }

    public Envelope? Envelope { get; protected set; }


    public async Task FlushOutgoingMessagesAsync()
    {
        foreach (var envelope in Outstanding)
        {
            try
            {
                await envelope.QuickSendAsync();
            }
            catch (Exception e)
            {
                Logger.LogException(e, envelope.CorrelationId,
                    "Unable to send an outgoing message, most likely due to serialization issues");
                Logger.DiscardedEnvelope(envelope);
            }
        }

        if (ReferenceEquals(Transaction, this))
        {
            await flushScheduledMessagesAsync();
        }

        _outstanding.Clear();
    }

    public async ValueTask UseInMemoryTransactionAsync()
    {
        if (!ReferenceEquals(this, Transaction))
        {
            await EnlistInTransactionAsync(this);
        }
    }

    public void EnlistInSaga(object? sagaId)
    {
        _sagaId = sagaId ?? throw new ArgumentNullException(nameof(sagaId));
        foreach (var envelope in _outstanding) envelope.SagaId = sagaId.ToString();
    }

    Envelope IAcknowledgementSender.BuildAcknowledgement(Envelope envelope)
    {
        return Runtime.Acknowledgements.BuildAcknowledgement(envelope);
    }

    Task IAcknowledgementSender.SendAcknowledgementAsync(Envelope envelope)
    {
        return Runtime.Acknowledgements.SendAcknowledgementAsync(envelope);
    }

    Task IAcknowledgementSender.SendFailureAcknowledgementAsync(Envelope original, string message)
    {
        return Runtime.Acknowledgements.SendFailureAcknowledgementAsync(original, message);
    }

    public ValueTask CompleteAsync()
    {
        if (_channel == null || Envelope == null)
        {
            throw new InvalidOperationException("No Envelope is active for this context");
        }

        return _channel.CompleteAsync(Envelope);
    }

    public ValueTask DeferAsync()
    {
        if (_channel == null || Envelope == null)
        {
            throw new InvalidOperationException("No Envelope is active for this context");
        }

        return _channel.DeferAsync(Envelope);
    }

    public async Task ReScheduleAsync(DateTimeOffset scheduledTime)
    {
        if (_channel == null || Envelope == null)
        {
            throw new InvalidOperationException("No Envelope is active for this context");
        }

        Envelope.ScheduledTime = scheduledTime;
        if (_channel is IHasNativeScheduling c)
        {
            await c.MoveToScheduledUntilAsync(Envelope, Envelope.ScheduledTime.Value);
        }
        else
        {
            await Persistence.ScheduleJobAsync(Envelope);
        }
    }

    public async Task MoveToDeadLetterQueueAsync(Exception exception)
    {
        if (_channel == null || Envelope == null)
        {
            throw new InvalidOperationException("No Envelope is active for this context");
        }

        if (_channel is IHasDeadLetterQueue c)
        {
            await c.MoveToErrorsAsync(Envelope, exception);
        }
        else
        {
            // If persistable, persist
            await Persistence.MoveToDeadLetterStorageAsync(Envelope, exception);
        }
    }

    public Task RetryExecutionNowAsync()
    {
        if (_channel == null || Envelope == null)
        {
            throw new InvalidOperationException("No Envelope is active for this context");
        }

        return Runtime.Pipeline.InvokeAsync(Envelope, _channel!);
    }

    internal void ClearState()
    {
        _outstanding.Clear();
        _scheduled.Clear();
        Envelope = null;
        Transaction = null;
        _sagaId = null;
    }

    internal void ReadEnvelope(Envelope? originalEnvelope, IChannelCallback channel)
    {
        Envelope = originalEnvelope ?? throw new ArgumentNullException(nameof(originalEnvelope));
        CorrelationId = originalEnvelope.CorrelationId;
        _channel = channel;
        _sagaId = originalEnvelope.SagaId;

        Transaction = this;

        if (Envelope.AckRequested)
        {
            var ack = Runtime.Acknowledgements.BuildAcknowledgement(Envelope);

            _outstanding.Add(ack);
        }
    }

    private async Task flushScheduledMessagesAsync()
    {
        if (Persistence is NullEnvelopePersistence)
        {
            foreach (var envelope in _scheduled) Runtime.ScheduleLocalExecutionInMemory(envelope.ScheduledTime!.Value, envelope);
        }
        else
        {
            foreach (var envelope in _scheduled) await Persistence.ScheduleJobAsync(envelope);
        }

        _scheduled.Clear();
    }

    protected override void trackEnvelopeCorrelation(Envelope outbound)
    {
        base.trackEnvelopeCorrelation(outbound);
        outbound.SagaId = _sagaId?.ToString() ?? Envelope?.SagaId ?? outbound.SagaId;

        if (Envelope != null)
        {
            outbound.CausationId = Envelope.Id.ToString();
        }
    }
}
