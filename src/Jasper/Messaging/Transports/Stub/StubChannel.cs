using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Stub
{
    public class StubChannel : ISendingAgent, IDisposable
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly StubTransport _stubTransport;
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


            _pipeline.Invoke(envelope).Wait();

            return Task.CompletedTask;
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

        public int QueuedCount { get; } = 0;
    }
}
