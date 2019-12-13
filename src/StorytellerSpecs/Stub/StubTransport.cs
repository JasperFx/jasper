using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;
using Jasper.Util;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Jasper.Messaging.Transports.Stub
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
                .GetRequiredService<IMessagingRoot>()
                .Options
                .Endpoints
                .As<TransportCollection>()
                .Get<StubTransport>();
        }

    }

    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubEndpoint> Endpoints;

        public StubTransport()
        {
            Endpoints =
                new LightweightCache<Uri, StubEndpoint>(u => new StubEndpoint(u, this));
        }

        public Endpoint ReplyEndpoint()
        {
            return Endpoints["stub://replies".ToUri()];
        }

        public Endpoint TryGetEndpoint(Uri uri)
        {
            return Endpoints.TryRetrieve(uri, out var endpoint) ? endpoint : null;
        }

        IEnumerable<Endpoint> ITransport.Endpoints()
        {
            return Endpoints;
        }

        public void Initialize()
        {
            // Nothing
        }

        public void Dispose()
        {

        }

        public string Protocol { get; } = "stub";

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            var pipeline = root.Pipeline;

            foreach (var channel in Endpoints)
            {
                channel.Start(pipeline, root.MessageLogger);

                runtime.AddSubscriber(channel, channel.Subscriptions.ToArray());
            }
        }

        public Endpoint GetOrCreateEndpoint(Uri uri)
        {
            return Endpoints[uri];
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var listener in _listeners) Endpoints.FillDefault(listener);
        }

        public IList<StubMessageCallback> Callbacks { get; } = new List<StubMessageCallback>();


        private readonly IList<Uri> _listeners = new List<Uri>();

        public Endpoint ListenTo(Uri uri)
        {
            _listeners.Add(uri);
            return null;
        }

        public StubMessageCallback LastCallback()
        {
            return Callbacks.LastOrDefault();
        }
    }
}
