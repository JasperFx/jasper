using System;
using System.Threading;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;

namespace Jasper.Bus.Transports
{
    // Will become the new ITransport
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation);

        Uri DefaultReplyUri();

        void StartListening(BusSettings settings);
    }
}
