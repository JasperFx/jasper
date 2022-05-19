using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Runtime;

namespace Jasper.Transports;


public interface ITransport
{
    ICollection<string> Protocols { get; }


    /// <summary>
    ///     Strictly a diagnostic name for this transport type
    /// </summary>
    string Name { get; }

    Endpoint? ReplyEndpoint();

    Endpoint ListenTo(Uri uri);

    void StartSenders(IJasperRuntime root);
    void StartListeners(IJasperRuntime root);

    Endpoint GetOrCreateEndpoint(Uri uri);
    Endpoint? TryGetEndpoint(Uri uri);

    IEnumerable<Endpoint> Endpoints();
    ValueTask InitializeAsync(IJasperRuntime runtime);
}
