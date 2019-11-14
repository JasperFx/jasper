using System;
using System.IO;
using System.Threading;
using Jasper.Messaging.Durability;
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
            if (uri.IsDurable())
            {
                var worker = new DurableWorkerQueue(new ListenerSettings(), root.Pipeline, root.Settings, root.Persistence, root.TransportLogger);
                return new DurableLoopbackSendingAgent(uri, worker, root.Persistence, root.Serialization, root.TransportLogger, root.Settings);
            }

            return new LoopbackSendingAgent(uri, new LightweightWorkerQueue(new ListenerSettings(), root.TransportLogger, root.Pipeline, root.Settings));
        }

        public Uri ReplyUri => TransportConstants.RetryUri;

        public void InitializeSendersAndListeners(IMessagingRoot root)
        {
            // nothing to do here
        }

    }
}
