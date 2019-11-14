using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
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
            return host.Services.GetRequiredService<IMessagingRoot>().Transports.OfType<StubTransport>().Single();
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

        public bool WasDisposed { get; set; }
        public IList<StubMessageCallback> Callbacks { get; } = new List<StubMessageCallback>();

        public void Dispose()
        {
            WasDisposed = true;
        }

        public string Protocol { get; } = "stub";

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            return Channels[uri];
        }

        public Uri ReplyUri { get; }

        public void InitializeSendersAndListeners(IMessagingRoot root)
        {
            var pipeline = root.Pipeline;

            Channels =
                new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri, pipeline, this));


            var incoming = root.Options.Listeners.Where(x => x.Uri.Scheme == "stub");
            foreach (var listener in incoming) Channels.FillDefault(listener.Uri);

            // TODO -- later this configuration will be directly on here
            var groups = root.Options.Subscriptions.Where(x => x.Uri.Scheme == Protocol).GroupBy(x => x.Uri);
            foreach (var @group in groups)
            {
                var subscriber = new Subscriber(@group.Key, @group);
                var agent = BuildSendingAgent(subscriber.Uri, root, root.Settings.Cancellation);


                subscriber.StartSending(root.Logger, agent, ReplyUri);

                root.AddSubscriber(subscriber);
            }
        }

        public StubMessageCallback LastCallback()
        {
            return Callbacks.LastOrDefault();
        }
    }
}
