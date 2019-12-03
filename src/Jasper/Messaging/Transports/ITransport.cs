using System;
using System.IO;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Uri ReplyUri { get; }

        Endpoint ListenTo(Uri uri);

        void StartSenders(IMessagingRoot root, ITransportRuntime runtime);
        void StartListeners(IMessagingRoot root, ITransportRuntime runtime);

        // TODO -- make this NOT take in the subscriptions
        Endpoint Subscribe(Uri uri, Subscription[] subscriptions);


        Endpoint DetermineEndpoint(Uri uri);
    }
}
