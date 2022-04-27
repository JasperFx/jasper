using System;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime
{
    internal class InvocationCallback : IChannelCallback, IHasNativeScheduling, IHasDeadLetterQueue
    {
        public static readonly InvocationCallback Instance = new InvocationCallback();

        private InvocationCallback()
        {
        }

        public ValueTask CompleteAsync(Envelope envelope)
        {
            return ValueTask.CompletedTask;
        }

        public Task MoveToErrorsAsync(Envelope envelope, Exception exception)
        {
            return Task.CompletedTask;
        }

        public ValueTask DeferAsync(Envelope envelope)
        {
            return ValueTask.CompletedTask;
        }

        public Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
        {
            return Task.CompletedTask;
        }
    }
}
