using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

        public Uri LocalReplyUri => TransportConstants.RetryUri;

        public void StartListening(BusSettings settings)
        {
            // Nothing really, since it's just a handoff to the internal worker queues
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("Listening for loopback messages");
        }
    }
}
