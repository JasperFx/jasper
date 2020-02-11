using System;
using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.Runtime
{
    internal class InvocationCallback : IMessageCallback
    {
        public Task Complete()
        {
            return Task.CompletedTask;
        }

        public Task MoveToErrors(Exception exception)
        {
            return Task.CompletedTask;
        }

        public Task Defer()
        {
            return Task.CompletedTask;
        }

        public Task MoveToScheduledUntil(DateTimeOffset time)
        {
            return Task.CompletedTask;
        }
    }
}
