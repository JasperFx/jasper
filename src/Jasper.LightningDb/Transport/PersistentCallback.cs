using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Bus.Delayed;
using Jasper.Bus.Queues;
using Jasper.Bus.Runtime;

namespace Jasper.LightningDb.Transport
{
    internal class PersistentCallback : IMessageCallback
    {
        private readonly Envelope _message;
        private readonly ActionBlock<Envelope> _block;

        public PersistentCallback(Envelope message, ActionBlock<Envelope> block)
        {
            _message = message;
            _block = block;
        }

        public Task MarkSuccessful()
        {
            throw new NotImplementedException();
        }

        public Task MarkFailed(Exception ex)
        {
            throw new NotImplementedException();
        }

        public Task MoveToDelayedUntil(Envelope envelope, IDelayedJobProcessor delayedJobs, DateTime time)
        {
            throw new NotImplementedException();
        }

        public Task MoveToErrors(ErrorReport report)
        {
            throw new NotImplementedException();
        }

        public Task Requeue(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Task Send(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public bool SupportsSend { get; }
    }
}
