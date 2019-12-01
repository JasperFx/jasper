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
            host.GetStubTransport().Channels.Each(x => x.Sent.Clear());
        }

        /// <summary>
        ///     Retrieves an array of all the envelopes sent through the stub transport
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        public static Envelope[] GetAllEnvelopesSent(this IHost host)
        {
            return host.GetStubTransport().Channels.SelectMany(x => x.Sent).ToArray();
        }
    }

    public class StubTransport : ITransport
    {
        public LightweightCache<Uri, StubChannel> Channels;

        public StubTransport()
        {
            ReplyUri = new Uri("stub://replies");
        }

        public void Dispose()
        {

        }

        public readonly IList<Subscription> Subscriptions = new List<Subscription>();

        public string Protocol { get; } = "stub";

        public void StartSenders(IMessagingRoot root, ITransportRuntime runtime)
        {
            var pipeline = root.Pipeline;

            Channels =
                new LightweightCache<Uri, StubChannel>(u => new StubChannel(u, pipeline, this));


            var groups = Subscriptions.GroupBy(x => x.Uri);
            foreach (var @group in groups)
            {
                var agent = Channels[@group.Key];
                runtime.AddSubscriber(agent, @group.ToArray());
            }
        }

        public void StartListeners(IMessagingRoot root, ITransportRuntime runtime)
        {
            foreach (var listener in _listeners) Channels.FillDefault(listener);
        }

        public ISender CreateSender(Uri uri, CancellationToken cancellation, IMessagingRoot root)
        {
            throw new NotSupportedException();
        }


        public void Subscribe(Subscription subscription)
        {
            Subscriptions.Add(subscription);
        }


        public bool WasDisposed { get; set; }
        public IList<StubMessageCallback> Callbacks { get; } = new List<StubMessageCallback>();


        public Uri ReplyUri { get; }

        private readonly IList<Uri> _listeners = new List<Uri>();

        public ListenerSettings ListenTo(Uri uri)
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
