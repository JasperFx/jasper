using System;
using System.Collections.Generic;
using Jasper.Configuration;
using Jasper.Runtime;

namespace Jasper.Transports
{
    public interface ITransport : IDisposable
    {
        ICollection<string> Protocols { get; }

        Endpoint ReplyEndpoint();

        Endpoint ListenTo(Uri uri);

        void StartSenders(IMessagingRoot root, ITransportRuntime runtime);
        void StartListeners(IMessagingRoot root, ITransportRuntime runtime);

        Endpoint GetOrCreateEndpoint(Uri uri);
        Endpoint TryGetEndpoint(Uri uri);

        IEnumerable<Endpoint> Endpoints();
        void Initialize(IMessagingRoot root);
    }
}
