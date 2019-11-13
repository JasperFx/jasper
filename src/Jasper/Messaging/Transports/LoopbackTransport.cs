using System;
using System.IO;
using System.Threading;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class LoopbackTransport : ITransport
    {
        public void Dispose()
        {
            // Nothing really
        }

        public string Protocol { get; } = TransportConstants.Loopback;

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
//            return uri.IsDurable()
//                ? root.BuildDurableLoopbackAgent(uri)
//                : new LoopbackSendingAgent(uri, roo);
        }

        public Uri ReplyUri => TransportConstants.RetryUri;

        public void InitializeSendersAndListeners(IMessagingRoot root)
        {
            // nothing to do here
        }

    }
}
