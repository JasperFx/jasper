using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Scheduled;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Testing.Messaging.Transports.Tcp
{
    public class FakeScheduledJobProcessor : IScheduledJobProcessor
    {
        private readonly TaskCompletionSource<Envelope> _envelope = new TaskCompletionSource<Envelope>();

        public void Enqueue(DateTimeOffset executionTime, Envelope envelope)
        {
            envelope.ExecutionTime = executionTime;
            _envelope.SetResult(envelope);
        }

        public Task PlayAll()
        {
            throw new NotImplementedException();
        }

        public Task PlayAt(DateTime executionTime)
        {
            throw new NotImplementedException();
        }

        public Task EmptyAll()
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public ScheduledJob[] QueuedJobs()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }

        public Task<Envelope> Envelope()
        {
            return _envelope.Task;
        }

        public void Start(IWorkerQueue workerQueue)
        {
        }
    }
}
