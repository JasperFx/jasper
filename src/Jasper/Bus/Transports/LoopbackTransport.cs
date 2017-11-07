using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;
using Jasper.Util;

namespace Jasper.Bus.Transports
{
    public class LoopbackTransport : ITransport
    {
        private readonly IPersistence _persistence;
        private readonly IWorkerQueue _workerQueue;

        public LoopbackTransport(IWorkerQueue workerQueue, IPersistence persistence)
        {
            _workerQueue = workerQueue;
            _persistence = persistence;
        }

        public void Dispose()
        {
            // Nothing really
        }

        public string Protocol => TransportConstants.Loopback;

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            return uri.IsDurable()
                ? _persistence.BuildLocalAgent(uri, _workerQueue)
                : new LoopbackSendingAgent(uri, _workerQueue);
        }

        public Uri DefaultReplyUri()
        {
            return TransportConstants.RetryUri;
        }

        public void StartListening(BusSettings settings)
        {
            // Nothing really, since it's just a handoff to the internal worker queues
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("Listening for loopback messages");
        }
    }

    public class LoopbackSendingAgent : ISendingAgent
    {
        private readonly IWorkerQueue _queues;
        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

        public LoopbackSendingAgent(Uri destination, IWorkerQueue queues)
        {
            _queues = queues;
            Destination = destination;
        }

        public void Dispose()
        {
            // Nothing
        }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;
            envelope.ReceivedAt = Destination;
            envelope.Callback = new LightweightCallback(_queues);

            if (envelope.IsDelayed(DateTime.UtcNow))
            {
                _queues.DelayedJobs.Enqueue(envelope.ExecutionTime.Value, envelope);
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

        public void Start()
        {
            // nothing
        }
    }
}
