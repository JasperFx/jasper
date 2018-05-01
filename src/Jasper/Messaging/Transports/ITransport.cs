using System;
using System.IO;
using System.Threading;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation);

        Uri LocalReplyUri { get; }

        void StartListening(IMessagingRoot root);

        void Describe(TextWriter writer);

        ListeningStatus ListeningStatus { get; set; }
    }
}
