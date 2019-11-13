using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Scheduled;

namespace Jasper.Messaging.WorkerQueues
{
    public class LightweightWorkerQueue : IWorkerQueue
    {
        private readonly ActionBlock<Envelope> _receiver;

        public LightweightWorkerQueue(ListenerSettings listenerSettings, ITransportLogger logger,
            IHandlerPipeline pipeline, AdvancedSettings settings1)
        {
            Pipeline = pipeline;

            ScheduledJobs = new InMemoryScheduledJobProcessor(this);

            listenerSettings.ExecutionOptions.CancellationToken = settings1.Cancellation;

            _receiver = new ActionBlock<Envelope>(async envelope =>
            {
                try
                {
                    envelope.ContentType = envelope.ContentType ?? "application/json";

                    await Pipeline.Invoke(envelope);
                }
                catch (Exception e)
                {
                    // This *should* never happen, but of course it will
                    logger.LogException(e);
                }
            }, listenerSettings.ExecutionOptions);
        }

        public IHandlerPipeline Pipeline { get; }

        public int QueuedCount => _receiver.InputCount;
        public IScheduledJobProcessor ScheduledJobs { get; }
        public Task Enqueue(Envelope envelope)
        {
            if (envelope.IsPing()) return Task.CompletedTask;

            envelope.Callback = new LightweightCallback(this);
            _receiver.Post(envelope);

            return Task.CompletedTask;
        }

        [Obsolete]
        public void AddQueue(string queueName, int parallelization)
        {
            throw new System.NotImplementedException();
        }

        public Task ScheduleExecution(Envelope envelope)
        {
            ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
            return Task.CompletedTask;
        }


    }
}