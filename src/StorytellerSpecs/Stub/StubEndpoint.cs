using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Jasper;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace StorytellerSpecs.Stub
{
    public class StubEndpoint : Endpoint, ISendingAgent, ISender, IDisposable
    {
        private readonly StubTransport _stubTransport;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public readonly IList<Envelope> Sent = new List<Envelope>();
        private ISenderCallback _callback;
        private IMessageLogger _logger;
        private IHandlerPipeline _pipeline;
        private Uri _replyUri;


        public StubEndpoint(Uri destination, StubTransport stubTransport)
        {
            _stubTransport = stubTransport;
            Destination = destination;
        }

        public override Uri Uri => $"stub://{Name}".ToUri();

        public int QueuedCount { get; }

        public void Start(ISenderCallback callback)
        {
            _callback = callback;
        }

        public Task Enqueue(Envelope envelope)
        {
            Sent.Add(envelope);
            return _pipeline?.Invoke(envelope) ?? Task.CompletedTask;
        }

        public Task LatchAndDrain()
        {
            return Task.CompletedTask;
        }

        public void Unlatch()
        {
            Latched = false;
        }

        public Task Ping()
        {
            return Task.CompletedTask;
        }


        public void Dispose()
        {
        }

        public bool Latched { get; set; }

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

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return this;
        }
    }
}
