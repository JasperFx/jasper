using System;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Logging;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;

namespace Jasper.Bus.Transports.WorkerQueues
{
    public interface IDurableWorkerQueue : IWorkerQueue{}

    public class DurableWorkerQueue : WorkerQueue, IDurableWorkerQueue
    {
        private readonly IPersistence _persistence;

        public DurableWorkerQueue(IPersistence persistence, CompositeLogger logger, IHandlerPipeline pipeline, CancellationToken cancellationToken) : base(logger, pipeline, cancellationToken)
        {
            _persistence = persistence;
        }

        protected override IMessageCallback buildCallback(Envelope envelope, string queueName)
        {
            return new DurableCallback(this, queueName, envelope);
        }

        public class DurableCallback : IMessageCallback
        {
            private readonly DurableWorkerQueue _parent;
            private readonly string _queueName;
            private readonly Envelope _envelope;

            public DurableCallback(DurableWorkerQueue parent, string queueName, Envelope envelope)
            {
                _parent = parent;
                _queueName = queueName;
                _envelope = envelope;
            }

            public Task MarkSuccessful()
            {
                _parent._persistence.Remove(_queueName, _envelope);
                return Task.CompletedTask;
            }

            public Task MarkFailed(Exception ex)
            {
                _parent._persistence.Remove(_queueName, _envelope);
                return Task.CompletedTask;
            }

            public Task MoveToErrors(ErrorReport report)
            {
                // There's an outstanding issue for actually doing error reports
                return Task.CompletedTask;
            }

            public Task Requeue(Envelope envelope)
            {
                return _parent.Enqueue(envelope);
            }

        }
    }


}
