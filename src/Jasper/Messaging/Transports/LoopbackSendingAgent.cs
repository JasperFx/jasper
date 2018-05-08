using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class LoopbackSendingAgent : ISendingAgent
    {
        private readonly IWorkerQueue _queues;
        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

        public LoopbackSendingAgent(Uri destination, IWorkerQueue queues)
        {
            _queues = queues ?? throw new ArgumentNullException(nameof(queues));
            Destination = destination;
        }

        public void Dispose()
        {
            // Nothing
        }

        public bool Latched { get; } = false;

        public bool IsDurable => Destination.IsDurable();

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
            envelope.ReceivedAt = Destination;
            envelope.Callback = new LightweightCallback(_queues);

            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                _queues.ScheduledJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
                return Task.CompletedTask;
            }
            else
            {
                return _queues.Enqueue(envelope);
            }


        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public void Start()
        {
            // nothing
        }

        public int QueuedCount { get; } = 0;
    }
}
