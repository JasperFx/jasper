using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Configuration;
using Jasper.Runtime;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Transports.Stub
{
    public static class StubTransportExtensions
    {
        /// <summary>
        ///     Retrieves the instance of the StubTransport within this application
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static StubTransport GetStubTransport(this IHost host)
        {
            return host
                .Services
                .GetRequiredService<IJasperRuntime>()
                .Options
                .Get<StubTransport>();
        }
    }

    public class StubTransport : TransportBase<StubEndpoint>
    {
        public new LightweightCache<Uri, StubEndpoint> Endpoints { get; }

        public StubTransport() : base("stub", "Stub")
        {
            Endpoints =
                new LightweightCache<Uri, StubEndpoint>(u => new StubEndpoint(u, this));
        }

        protected override IEnumerable<StubEndpoint> endpoints()
        {
            return Endpoints.GetAll();
        }

        protected override StubEndpoint findEndpointByUri(Uri uri)
        {
            return Endpoints[uri];
        }

        public override void Initialize(IJasperRuntime root)
        {
            foreach (var endpoint in Endpoints)
            {
                endpoint.Start(root.Pipeline, root.MessageLogger);
            }
        }
    }
}
