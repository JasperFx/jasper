using System;
using System.IO;
using System.Threading;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports
{
    public class LoopbackTransport : ITransport
    {

        public void Dispose()
        {
            // Nothing really
        }

        public string Protocol => TransportConstants.Loopback;

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            return uri.IsDurable()
                ? root.Factory.BuildLocalAgent(uri, root)
                : new LoopbackSendingAgent(uri, root.Workers);
        }

        public Uri LocalReplyUri => TransportConstants.RetryUri;

        public void StartListening(IMessagingRoot root)
        {
            // nothing to do here
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("Listening for loopback messages");
        }
    }
}
