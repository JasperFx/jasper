using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Jasper.Configuration;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Endpoint ReplyEndpoint();

        Endpoint ListenTo(Uri uri);

        void StartSenders(IMessagingRoot root, ITransportRuntime runtime);
        void StartListeners(IMessagingRoot root, ITransportRuntime runtime);

        Endpoint GetOrCreateEndpoint(Uri uri);
        Endpoint TryGetEndpoint(Uri uri);

        IEnumerable<Endpoint> Endpoints();
        void Initialize();
    }
}
