using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;

namespace Jasper.Bus
{
    public partial class ServiceBus
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

            public Task MoveToDelayedUntil(DateTimeOffset time, Envelope envelope)
            {
                return Task.CompletedTask;
            }
        }
    }
}