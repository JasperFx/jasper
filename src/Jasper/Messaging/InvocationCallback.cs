using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;

namespace Jasper.Messaging
{
    internal class InvocationCallback : IMessageCallback
    {
        public Task MarkComplete()
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(Envelope envelope, Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            return Task.CompletedTask;
        }

        public Task MoveToScheduledUntil(DateTimeOffset time, Envelope envelope)
        {
            return Task.CompletedTask;
        }
    }
}
