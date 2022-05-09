﻿using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Durability;
using Jasper.Runtime;

namespace Jasper;

public interface IExecutionContext : IMessagePublisher, IAcknowledgementSender
{
    /// <summary>
    ///     Correlating identifier for the logical workflow. All envelopes sent or executed
    ///     through this context will be tracked with this identifier. If this context is the
    ///     result of a received message, this will be the original Envelope.CorrelationId
    /// </summary>
    string? CorrelationId { get; }

    /// <summary>
    ///     The envelope being currently handled. This will only be non-null during
    ///     the handling of a message
    /// </summary>
    Envelope? Envelope { get; }

    /// <summary>
    ///     The active envelope persistence for the application. This is used
    ///     by the "outbox" support in Jasper
    /// </summary>
    IEnvelopePersistence? Persistence { get; }


    IMessageLogger Logger { get; }

    /// <summary>
    ///     Current outbox transaction
    /// </summary>
    IEnvelopeTransaction? Transaction { get; }

    /// <summary>
    ///     If a messaging context is enlisted in a transaction, calling this
    ///     method will force the context to send out any outstanding messages
    ///     that were captured as part of processing the transaction
    /// </summary>
    /// <returns></returns>
    Task FlushOutgoingMessagesAsync();

    /// <summary>
    ///     Enlist this context within some kind of existing business
    ///     transaction so that messages are only sent if the transaction succeeds.
    ///     Jasper's "Outbox" support
    /// </summary>
    /// <param name="transaction"></param>
    /// <returns></returns>
    Task EnlistInTransactionAsync(IEnvelopeTransaction transaction);


    /// <summary>
    ///     Opt into using an in memory transaction for the execution context.
    ///     Honestly, this is mostly for testing usage
    /// </summary>
    void UseInMemoryTransaction();

    /// <summary>
    ///     Called by Jasper itself to mark this context as being part
    ///     of a stateful saga
    /// </summary>
    /// <param name="sagaId"></param>
    void EnlistInSaga(object? sagaId);

    /// <summary>
    ///     Send a response message back to the original sender of the message being handled.
    ///     This can only be used from within a message handler
    /// </summary>
    /// <param name="context"></param>
    /// <param name="response"></param>
    /// <returns></returns>
    // TODO -- this should probably be ValueTask
    Task RespondToSenderAsync(object response);

    /// <summary>
    ///     Enqueue a cascading message to the outstanding context transaction
    ///     Can be either the message itself, any kind of ISendMyself object,
    ///     or an IEnumerable<object>
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    Task EnqueueCascadingAsync(object message);


    /// <summary>
    ///     Mark the message as having been successfully received and processed
    /// </summary>
    /// <param name="envelope"></param>
    /// <returns></returns>
    ValueTask CompleteAsync();

    /// <summary>
    ///     Requeue the message for later processing
    /// </summary>
    /// <param name="envelope"></param>
    /// <returns></returns>
    ValueTask DeferAsync();

    Task ReScheduleAsync(DateTimeOffset scheduledTime);

    Task MoveToDeadLetterQueueAsync(Exception exception);

    /// <summary>
    ///     Immediately execute the message again
    /// </summary>
    /// <returns></returns>
    Task RetryExecutionNowAsync();
}
