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

        public LoopbackSendingAgent(Uri destination, IWorkerQueue queues)
        {
            _queues = queues ?? throw new ArgumentNullException(nameof(queues));
            Destination = destination;
        }

        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

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

            return envelope.IsDelayed(DateTime.UtcNow)
                ? _queues.ScheduleExecution(envelope)
                : _queues.Enqueue(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public void Start()
        {
            // nothing
        }

        public bool SupportsNativeScheduledSend { get; } = true;

    }
}
