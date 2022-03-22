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

        public Task CompleteAsync(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrorsAsync(Envelope envelope, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task DeferAsync(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToScheduledUntilAsync(Envelope envelope, DateTimeOffset time)
        {
            return Task.CompletedTask;
        }
    }
}
