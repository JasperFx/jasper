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

        // TODO -- later!
        //void AssertUriIsValid(Uri uri);
        //Uri CanonicizeUri(Uri uri);

        Uri ReplyUri { get; }

        Endpoint ListenTo(Uri uri);

        void StartSenders(IMessagingRoot root, ITransportRuntime runtime);
        void StartListeners(IMessagingRoot root, ITransportRuntime runtime);

        void Subscribe(Uri uri, Subscription subscription);


        Endpoint DetermineEndpoint(Uri uri);
    }
}
