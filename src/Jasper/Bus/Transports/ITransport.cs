using System;
using System.IO;
using System.Threading;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Bus.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation);

        Uri LocalReplyUri { get; }

        void StartListening(BusSettings settings, IWorkerQueue workers);

        void Describe(TextWriter writer);
    }
}
