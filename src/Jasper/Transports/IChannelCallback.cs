using System;
using System.Threading.Tasks;

namespace Jasper.Transports;

/// <summary>
///     Marks an IChannelCallback as supporting a native dead letter queue
///     functionality
/// </summary>
public interface IHasDeadLetterQueue
{
    Task MoveToErrorsAsync(Envelope envelope, Exception exception);
}

/// <summary>
///     Marks an IChannelCallback as supporting native scheduled send
/// </summary>
public interface IHasNativeScheduling
{
    /// <summary>
    ///     Move the current message represented by the envelope to a
    ///     scheduled delivery
    /// </summary>
    /// <param name="envelope"></param>
    /// <param name="time"></param>
    /// <returns></returns>
    Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time);
}

public interface IChannelCallback
{
    /// <summary>
    ///     Mark the message as having been successfully received and processed
    /// </summary>
    /// <param name="envelope"></param>
    /// <returns></returns>
    ValueTask CompleteAsync(Envelope envelope);


    /// <summary>
    ///     Requeue the message for later processing
    /// </summary>
    /// <param name="envelope"></param>
    /// <returns></returns>
    ValueTask DeferAsync(Envelope envelope);
}
