using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Scheduled;

namespace Jasper.Messaging.WorkerQueues
{
    public class DurableWorkerQueue : IWorkerQueue
    {
        private readonly IEnvelopePersistence _persistence;
        private readonly ITransportLogger _transportLogger;
        private readonly ActionBlock<Envelope> _receiver;

        public DurableWorkerQueue(ListenerSettings listenerSettings, IHandlerPipeline pipeline,
            AdvancedSettings settings1, IEnvelopePersistence persistence, ITransportLogger transportLogger)
        {
            _persistence = persistence;
            _transportLogger = transportLogger;

            listenerSettings.ExecutionOptions.CancellationToken = settings1.Cancellation;

            _receiver = new ActionBlock<Envelope>(async envelope =>
            {
                try
                {
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    await pipeline.Invoke(envelope);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    transportLogger.LogException(e);
                }
            }, listenerSettings.ExecutionOptions);
        }

        public int QueuedCount => _receiver.InputCount;
        public Task Enqueue(Envelope envelope)
        {
            envelope.Callback = new DurableCallback(envelope, this, _persistence, _transportLogger);
            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        public Task ScheduleExecution(Envelope envelope)
        {
            return Task.CompletedTask;
        }

    }
}
