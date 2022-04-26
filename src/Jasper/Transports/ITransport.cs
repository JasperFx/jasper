using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime;

namespace Jasper.Transports
{
    public interface ITransport : IDisposable
    {
        ICollection<string> Protocols { get; }

        Endpoint ReplyEndpoint();

        Endpoint ListenTo(Uri? uri);

        void StartSenders(IJasperRuntime root, ITransportRuntime runtime);
        void StartListeners(IJasperRuntime root, ITransportRuntime runtime);

        Endpoint GetOrCreateEndpoint(Uri? uri);
        Endpoint TryGetEndpoint(Uri? uri);

        IEnumerable<Endpoint> Endpoints();
        Task Initialize(IJasperRuntime root);


        /// <summary>
        /// Strictly a diagnostic name for this transport type
        /// </summary>
        string Name { get; }
    }
}
