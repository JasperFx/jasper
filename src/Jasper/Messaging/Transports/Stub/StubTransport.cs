using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.WorkerQueues;

namespace Jasper.Messaging.Transports.Stub
{
    public static class StubTransportExtensions
    {
        /// <summary>
        ///     Retrieves the instance of the StubTransport within this application
        /// </summary>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public static StubTransport GetStubTransport(this JasperRuntime runtime)
        {
            return runtime.Get<IMessagingRoot>().Transports.OfType<StubTransport>().Single();
        }

        /// <summary>
        ///     Clears all record of messages sent to the stub transport
        /// </summary>
        /// <param name="runtime"></param>
        public static void ClearStubTransportSentList(this JasperRuntime runtime)
        {
            runtime.GetStubTransport().Channels.Each(x => x.Sent.Clear());
        }

        /// <summary>
        ///     Retrieves an array of all the envelopes sent through the stub transport
        /// </summary>
        /// <param name="runtime"></param>
        /// <returns></returns>
        public static Envelope[] AllSentThroughTheStubTransport(this JasperRuntime runtime)
        {
            return runtime.GetStubTransport().Channels.SelectMany(x => x.Sent).ToArray();
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

        public void StartListening(IMessagingRoot root)
        {
            var pipeline = root.Workers.As<WorkerQueue>().Pipeline;

            Channels =
                new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri, pipeline, this));


            var incoming = root.Settings.Listeners.Where(x => x.Scheme == "stub");
            foreach (var uri in incoming) Channels.FillDefault(uri);
        }

        public void Describe(TextWriter writer)
        {
            writer.WriteLine("'Stub' transport is active");
        }

        // Not really used here
        public ListeningStatus ListeningStatus { get; set; } = ListeningStatus.Accepting;

        public StubMessageCallback LastCallback()
        {
            return Callbacks.LastOrDefault();
        }
    }
}
