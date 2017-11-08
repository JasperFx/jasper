using System;
using System.IO;
using System.Threading;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;

namespace Jasper.Bus.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation);

        Uri LocalReplyUri();

        void StartListening(BusSettings settings);

        void Describe(TextWriter writer);
    }
}
