using System;
using System.Threading.Tasks;
using Jasper.Logging;
using Jasper.Persistence.Testing.Marten.Durability.App;
using Microsoft.Extensions.Logging;

namespace Jasper.Persistence.Testing.Marten.Durability
{
    public class SenderLatchDetected : TransportLogger
    {
        public TaskCompletionSource<bool> Waiter = new TaskCompletionSource<bool>();

        public SenderLatchDetected(ILoggerFactory factory) : base(factory, new NulloMetrics())
        {
        }

        public Task<bool> Received => Waiter.Task;

        public override void CircuitResumed(Uri destination)
        {
            if (destination == ReceiverApp.Listener) Waiter.TrySetResult(true);

            base.CircuitResumed(destination);
        }

        public override void CircuitBroken(Uri destination)
        {
            if (destination == ReceiverApp.Listener) Reset();

            base.CircuitBroken(destination);
        }

        public void Reset()
        {
            Waiter = new TaskCompletionSource<bool>();
        }
    }
}