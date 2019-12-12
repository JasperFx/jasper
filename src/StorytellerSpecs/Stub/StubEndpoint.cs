using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Runtime.Invocation;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports.Stub
{
    public class StubEndpoint : Endpoint, ISendingAgent, ISender, IDisposable
    {
        private IHandlerPipeline _pipeline;
        private readonly StubTransport _stubTransport;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public readonly IList<Envelope> Sent = new List<Envelope>();
        private Uri _replyUri;
        private IMessageLogger _logger;
        private ISenderCallback _callback;


        public StubEndpoint(Uri destination, StubTransport stubTransport)
        {
            _stubTransport = stubTransport;
            Destination = destination;
        }

        public void Start(IHandlerPipeline pipeline, IMessageLogger logger)
        {
            _pipeline = pipeline;
            _logger = logger;
        }

        public override Uri ReplyUri()
        {
            return _replyUri;
        }

        public override void Parse(Uri uri)
        {
            // Nothing
        }

        protected override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return this;
        }


        public void Dispose()
        {
        }

        public int QueuedCount { get; }
        public bool Latched { get; set; } = false;
        public void Start(ISenderCallback callback)
        {
            _callback = callback;
        }

        public Task Enqueue(Envelope envelope)
        {
            return _pipeline.Invoke(envelope);
        }

        public Task LatchAndDrain()
        {
            throw new NotImplementedException();
        }

        public void Unlatch()
        {
            throw new NotImplementedException();
        }

        public Task Ping()
        {
            throw new NotImplementedException();
        }

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

            _logger.Sent(envelope);

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
