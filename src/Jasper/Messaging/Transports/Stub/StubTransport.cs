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
            return host.Services.GetRequiredService<IMessagingRoot>().Options.Transports.Get<StubTransport>();
        }

        /// <summary>
        ///     Clears all record of messages sent to the stub transport
        /// </summary>
        /// <param name="host"></param>
        public static void ClearStubTransportSentList(this IHost host)
        {
            host.GetStubTransport().Endpoints.Each(x => x.Sent.Clear());
        }

        /// <summary>
        ///     Retrieves an array of all the envelopes sent through the stub transport
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Envelope[] GetAllEnvelopesSent(this IHost host)
        {
            return host.GetStubTransport().Endpoints.SelectMany(x => x.Sent).ToArray();
        }
    }

    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubEndpoint> Endpoints;

        public StubTransport()
        {
            ReplyUri = new Uri("stub://replies");
            Endpoints =
                new LightweightCache<Uri, StubEndpoint>(u => new StubEndpoint(u, this));
        }

        public Endpoint TryGetEndpoint(Uri uri)
        {
            return Endpoints.TryRetrieve(uri, out var endpoint) ? endpoint : null;
        }

        IEnumerable<Endpoint> ITransport.Endpoints()
        {
            return Endpoints;
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
                channel.Start(pipeline);

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


        public Uri ReplyUri { get; }

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
