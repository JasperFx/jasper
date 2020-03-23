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

        public Task Complete(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task Defer(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToScheduledUntil(Envelope envelope, DateTimeOffset time)
        {
            return Task.CompletedTask;
        }
    }
}
