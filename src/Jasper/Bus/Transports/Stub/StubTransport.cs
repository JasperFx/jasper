using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Baseline;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Sending;

namespace Jasper.Bus.Transports.Stub
{
    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels;
        private readonly IHandlerPipeline _pipeline;
        private Uri _replyUri;

        public StubTransport(IHandlerPipeline pipeline)

        {
            _pipeline = pipeline;
            _replyUri = new Uri($"stub://replies");

            Channels =
                new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri, pipeline, this));


        }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public string Protocol { get; } = "stub";
        public IList<StubMessageCallback> Callbacks { get; } = new List<StubMessageCallback>();

        public ISendingAgent BuildSendingAgent(Uri uri, CancellationToken cancellation)
        {
            return Channels[uri];
        }

        public Uri DefaultReplyUri()
        {
            return _replyUri;
        }

        public void StartListening(BusSettings settings)
        {
            var incoming = settings.Listeners.Where(x => x.Scheme == "stub");
            foreach (var uri in incoming)
            {
                Channels.FillDefault(uri);
            }
        }

        public StubMessageCallback LastCallback()
        {
            return Callbacks.LastOrDefault();
        }
    }
}
