using System;
using System.IO;
using System.Threading;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Uri ReplyUri { get; }

        ListeningStatus ListeningStatus { get; set; }

        ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation);

        void StartListening(IMessagingRoot root);

        void Describe(TextWriter writer);
    }
}
