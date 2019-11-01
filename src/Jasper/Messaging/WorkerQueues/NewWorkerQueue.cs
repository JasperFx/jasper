using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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

        public DurableWorkerQueue(ListenerSettings settings, IHandlerPipeline pipeline, JasperOptions options, IEnvelopePersistence persistence, ITransportLogger transportLogger)
        {
            _persistence = persistence;
            _transportLogger = transportLogger;
            ScheduledJobs = new InMemoryScheduledJobProcessor(this);

            settings.ExecutionOptions.CancellationToken = options.Cancellation;

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
            }, settings.ExecutionOptions);
        }

        public int QueuedCount => _receiver.InputCount;
        public IScheduledJobProcessor ScheduledJobs { get; }
        public Task Enqueue(Envelope envelope)
        {
            envelope.Callback = new DurableCallback(envelope, this, _persistence, _transportLogger);
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
            return Task.CompletedTask;
        }

    }

    public class LightweightWorkerQueue : IWorkerQueue
    {
        private readonly ActionBlock<Envelope> _receiver;

        public LightweightWorkerQueue(ListenerSettings settings, IMessageLogger logger, IHandlerPipeline pipeline, JasperOptions options)
        {
            Pipeline = pipeline;

            ScheduledJobs = new InMemoryScheduledJobProcessor(this);

            settings.ExecutionOptions.CancellationToken = options.Cancellation;

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
            }, settings.ExecutionOptions);
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
