using System;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime;

internal class InvocationCallback : IChannelCallback, IHasNativeScheduling, IHasDeadLetterQueue
{
    public static readonly InvocationCallback Instance = new();

    private InvocationCallback()
    {
    }

    public ValueTask CompleteAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public ValueTask DeferAsync(Envelope envelope)
    {
        return ValueTask.CompletedTask;
    }

    public Task MoveToErrorsAsync(Envelope envelope, Exception exception)
    {
        return Task.CompletedTask;
    }

    public Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
    {
        return Task.CompletedTask;
    }
}
