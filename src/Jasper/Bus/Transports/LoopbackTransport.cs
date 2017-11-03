using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.Transports.WorkerQueues;
using Jasper.Util;

namespace Jasper.Bus.Transports
{
    public class LoopbackTransport : ITransport
    {

        private readonly ILightweightWorkerQueue _lightweightWorkerQueue;
        private readonly IDurableWorkerQueue _workerQueue;
        private readonly IPersistence _persistence;

        public LoopbackTransport(ILightweightWorkerQueue lightweightWorkerQueue, IDurableWorkerQueue workerQueue, IPersistence persistence)
        {
            _lightweightWorkerQueue = lightweightWorkerQueue;
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
                ? (ISendingAgent) new DurableSendingAgent(uri, this)
                : new LoopbackSendingAgent(uri, this);
        }

        public Uri DefaultReplyUri()
        {
            return TransportConstants.RetryUri;
        }

        public void StartListening(BusSettings settings)
        {
            // Nothing really, since it's just a handoff to the internal worker queues
        }

        private class LoopbackSendingAgent : ISendingAgent
        {
            private readonly LoopbackTransport _parent;
            public Uri Destination { get; }
            public Uri DefaultReplyUri { get; set; }

            public LoopbackSendingAgent(Uri destination, LoopbackTransport parent)
            {
                _parent = parent;
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
                return _parent._lightweightWorkerQueue.Enqueue(envelope);
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

        private class DurableSendingAgent : ISendingAgent
        {
            private readonly LoopbackTransport _parent;
            public Uri Destination { get; }

            public DurableSendingAgent(Uri destination, LoopbackTransport parent)
            {
                _parent = parent;
                Destination = destination;
            }

            public void Dispose()
            {
                // nothing
            }

            public Uri DefaultReplyUri { get; set; }

            public Task EnqueueOutgoing(Envelope envelope)
            {
                envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;

                return _parent._workerQueue.Enqueue(envelope);
            }

            public Task StoreAndForward(Envelope envelope)
            {
                // TODO -- go async, and get an overload w/ a single envelope
                // please.
                _parent._persistence.StoreInitial(new Envelope[]{envelope});

                return EnqueueOutgoing(envelope);
            }

            public void Start()
            {
                // nothing
            }
        }
    }
}
