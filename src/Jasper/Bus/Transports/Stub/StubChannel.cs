using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Transports.Sending;

namespace Jasper.Bus.Transports.Stub
{
    public class StubChannel : ISendingAgent, IDisposable
    {
        private readonly StubTransport _stubTransport;
        private readonly IHandlerPipeline _pipeline;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public StubChannel(Uri destination, IHandlerPipeline pipeline, StubTransport stubTransport)
        {
            _pipeline = pipeline;
            _stubTransport = stubTransport;
            Destination = destination;
        }

        public readonly IList<Envelope> Sent = new List<Envelope>();

        public void Dispose()
        {

        }

        public bool Latched { get; set; } = false;

        public bool IsDurable => false;

        public Uri Destination { get; }
        public Uri DefaultReplyUri { get; set; }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReceivedAt = Destination;
            envelope.ReplyUri = envelope.ReplyUri ?? DefaultReplyUri;

            var callback = new StubMessageCallback(this);
            Callbacks.Add(callback);

            _stubTransport.Callbacks.Add(callback);

            Sent.Add(envelope);


            envelope.Callback = callback;

            envelope.ReceivedAt = Destination;

            return _pipeline.Invoke(envelope);
        }

        public Task StoreAndForward(Envelope envelope)
        {
            return EnqueueOutgoing(envelope);
        }

        public async Task StoreAndForwardMany(IEnumerable<Envelope> envelopes)
        {
            foreach (var envelope in envelopes)
            {
                await EnqueueOutgoing(envelope);
            }
        }

        public void Start()
        {

        }
    }
}
