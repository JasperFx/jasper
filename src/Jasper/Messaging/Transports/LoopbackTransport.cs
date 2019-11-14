using System;
using System.IO;
using System.Linq;
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
            // TODO -- later this configuration will be directly on here
            var groups = root.Options.Subscriptions.Where(x => x.Uri.Scheme == Protocol).GroupBy(x => x.Uri);
            foreach (var @group in groups)
            {
                var subscriber = new Subscriber(@group.Key, @group);
                var agent = BuildSendingAgent(subscriber.Uri, root, root.Settings.Cancellation);


                subscriber.StartSending(root.Logger, agent, ReplyUri);

                root.AddSubscriber(subscriber);
            }
        }

    }
}
