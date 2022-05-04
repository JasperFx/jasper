using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Runtime;

namespace Jasper.Transports.Stub;

public class StubTransport : TransportBase<StubEndpoint>
{
    public StubTransport() : base("stub", "Stub")
    {
        Endpoints =
            new LightweightCache<Uri, StubEndpoint>(u => new StubEndpoint(u, this));
    }

    public new LightweightCache<Uri, StubEndpoint> Endpoints { get; }

    protected override IEnumerable<StubEndpoint> endpoints()
    {
        return Endpoints.GetAll();
    }

    protected override StubEndpoint findEndpointByUri(Uri uri)
    {
        return Endpoints[uri];
    }

    public override ValueTask InitializeAsync(IJasperRuntime root)
    {
        foreach (var endpoint in Endpoints) endpoint.Start(root.Pipeline, root.MessageLogger);

        return ValueTask.CompletedTask;
    }
}
