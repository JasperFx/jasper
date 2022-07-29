using System;
using System.Collections.Generic;
using Jasper.Runtime;

namespace Jasper.ErrorHandling;

public interface IContinuationFactory
{
    /// <summary>
    ///     Use a custom IContinuation factory for matching exceptions
    /// </summary>
    /// <param name="continuationSource"></param>
    void ContinueWith(Func<Envelope, Exception, IContinuation> continuationSource);

    /// <summary>
    ///     Retry the message a maximum number of attempts without any delay
    ///     or moving the message back to the original queue
    /// </summary>
    void RetryNow();

    /// <summary>
    ///     Requeue the message back to the incoming transport on this attempt
    /// </summary>
    void Requeue();

    /// <summary>
    ///     Schedule the message for additional attempts with a delay. Use this
    ///     method to effect an "exponential backoff" policy
    /// </summary>
    void ScheduleRetry(TimeSpan delay);

    /// <summary>
    ///     Immediately move the message to the error queue when the exception
    ///     caught matches this criteria
    /// </summary>
    void MoveToErrorQueue();

    /// <summary>
    ///     Discard the message without any further attempt to process the message
    /// </summary>
    void Discard();
}

public class ContinuationFactory : IContinuationFactory
{
    private readonly IList<Func<Envelope, Exception, IContinuation>> _sources
        = new List<Func<Envelope, Exception, IContinuation>>();

    /// <summary>
    ///     Use a custom IContinuation factory for matching exceptions
    /// </summary>
    /// <param name="continuationSource"></param>
    public void ContinueWith(Func<Envelope, Exception, IContinuation> continuationSource)
    {
        _sources.Add(continuationSource);
    }

    /// <summary>
    ///     Retry the message a maximum number of attempts without any delay
    ///     or moving the message back to the original queue
    /// </summary>
    public void RetryNow()
    {
        _sources.Add((_, _) => RetryNowContinuation.Instance);
    }

    /// <summary>
    ///     Requeue the message back to the incoming transport on this attempt
    /// </summary>
    public void Requeue()
    {
        _sources.Add((_, _) => RequeueContinuation.Instance);
    }

    /// <summary>
    ///     Schedule the message for additional attempts with a delay. Use this
    ///     method to effect an "exponential backoff" policy
    /// </summary>
    public void ScheduleRetry(TimeSpan delay)
    {
        _sources.Add((_, _) => new ScheduledRetryContinuation(delay));
    }

    /// <summary>
    ///     Immediately move the message to the error queue when the exception
    ///     caught matches this criteria
    /// </summary>
    public void MoveToErrorQueue()
    {
        _sources.Add((_, ex) => new MoveToErrorQueue(ex));
    }

    /// <summary>
    ///     Discard the message without any further attempt to process the message
    /// </summary>
    public void Discard()
    {
        _sources.Add((_, _) => DiscardEnvelope.Instance);
    }

    public IContinuation Build(Envelope envelope, Exception ex)
    {
        // Shouldn't be necessary, but still
        if (envelope.Attempts == 0)
        {
            envelope.Attempts = 1;
        }

        return envelope.Attempts > _sources.Count
            ? new MoveToErrorQueue(ex)
            : _sources[envelope.Attempts - 1](envelope, ex);
    }
}
