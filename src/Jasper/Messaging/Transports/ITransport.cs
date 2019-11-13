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

        ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation);

        void InitializeSendersAndListeners(IMessagingRoot root);
    }
}
