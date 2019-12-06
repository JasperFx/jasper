using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Stub
{
    public class StubEndpoint : Endpoint, ISendingAgent, IDisposable
    {
        private IHandlerPipeline _pipeline;
        private readonly StubTransport _stubTransport;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public readonly IList<Envelope> Sent = new List<Envelope>();
        private Uri _replyUri;


        public StubEndpoint(Uri destination, StubTransport stubTransport)
        {
            _stubTransport = stubTransport;
            Destination = destination;
        }

        public void Start(IHandlerPipeline pipeline)
        {
            _pipeline = pipeline;
        }

        public override Uri ReplyUri()
        {
            return _replyUri;
        }

        public override void Parse(Uri uri)
        {
            // Nothing
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }

        protected internal override ISendingAgent StartSending(IMessagingRoot root, ITransportRuntime runtime, Uri replyUri)
        {
            return this;
        }

        public void Dispose()
        {
        }

        public bool Latched { get; set; } = false;

        public Uri Destination { get; }

        Uri ISendingAgent.ReplyUri
        {
            get => _replyUri;
            set => _replyUri = value;
        }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReceivedAt = Destination;
            envelope.ReplyUri = envelope.ReplyUri ?? ReplyUri();

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

        public bool SupportsNativeScheduledSend { get; } = true;

    }
}
