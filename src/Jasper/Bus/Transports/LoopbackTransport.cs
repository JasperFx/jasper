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
        private IWorkerQueue _workerQueue;

        public LoopbackTransport(IPersistence persistence)
        {
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

        public void StartListening(BusSettings settings, IWorkerQueue workers)
        {
            _workerQueue = workers;
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("Listening for loopback messages");
        }
    }
}
