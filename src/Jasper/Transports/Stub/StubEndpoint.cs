using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Configuration;
using Jasper.Logging;
using Jasper.Runtime;
using Jasper.Transports.Sending;
using Jasper.Util;

namespace Jasper.Transports.Stub
{
    public class StubEndpoint : Endpoint, ISendingAgent, ISender, IDisposable
    {
        private readonly StubTransport _stubTransport;
        public readonly IList<StubChannelCallback> Callbacks = new List<StubChannelCallback>();

        public readonly IList<Envelope> Sent = new List<Envelope>();
        private IMessageLogger _logger;
        private IHandlerPipeline _pipeline;


        public StubEndpoint(Uri destination, StubTransport stubTransport) : base(destination)
        {
            _stubTransport = stubTransport;
            Destination = destination;
            Agent = this;
        }

        public Endpoint Endpoint => this;
        public bool Latched { get; set; }
        public bool IsDurable => Mode == EndpointMode.Durable;

        public override Uri Uri => $"stub://{Name}".ToUri();

        public Task Send(Envelope envelope)
        {
            Sent.Add(envelope);
            return _pipeline?.Invoke(envelope, new StubChannelCallback(this, envelope)) ?? Task.CompletedTask;
        }

        public Task<bool> Ping(CancellationToken cancellationToken) => Task.FromResult(true);

        public void Dispose()
        {
        }

        public Uri Destination { get; }

        Uri ISendingAgent.ReplyUri
        {
            get => _stubTransport.ReplyEndpoint().Uri;
            set => Debug.WriteLine(value);
        }

        public Task EnqueueOutgoing(Envelope envelope)
        {
            envelope.ReplyUri ??= ReplyUri();

            var callback = new StubChannelCallback(this, envelope);
            Callbacks.Add(callback);

            _stubTransport.Callbacks.Add(callback);

            Sent.Add(envelope);

            _logger?.Sent(envelope);

            _pipeline?.Invoke(envelope, callback).Wait();

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
            return _stubTransport.ReplyEndpoint()?.Uri;
        }

        public override void Parse(Uri uri)
        {
            Name = uri.Host;
        }

        public override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            // Nothing
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            return this;
        }
    }
}
