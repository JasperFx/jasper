using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Settings;
using Jasper.Bus.Transports;
using Jasper.Util;

namespace Jasper.Testing.Bus.Stubs
{
    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels = new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri));

        public StubTransport(string scheme = "stub")
        {
            ReplyChannel = Channels[new Uri($"{scheme}://replies")];
            Protocol = scheme;
        }

        public StubChannel ReplyChannel { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public string Protocol { get; }
        public Uri ReplyUriFor(Uri node)
        {
            return ReplyChannel.Address;
        }

        public Task Send(Envelope envelope, Uri destination)
        {
            return Channels[destination].Send(envelope.Data, envelope.Headers);
        }


        public Uri CorrectedAddressFor(Uri address)
        {
            return address;
        }

        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            return new IChannel[0];
        }

        public Uri DefaultReplyUri()
        {
            return "stub://replies".ToUri();
        }

        public TransportState State { get; } = TransportState.Enabled;

        public bool Enabled { get; } = true;
    }

    public class StubChannel
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public StubChannel(Uri address)
        {
            Address = address;
        }

        public Uri Address { get; }

        public void StartReceiving(Receiver receiver)
        {
            Receiver = receiver;
        }

        public Receiver Receiver { get; set; }

        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public Task Send(byte[] data, IDictionary<string, string> headers)
        {
            var callback = new StubMessageCallback();
            Callbacks.Add(callback);
            return Receiver?.Receive(data, headers, callback) ?? Task.CompletedTask;
        }
    }

    public class StubMessageCallback : IMessageCallback
    {
        public readonly IList<Envelope> Sent = new List<Envelope>();
        public readonly IList<ErrorReport> Errors = new List<ErrorReport>();

        public Task MarkSuccessful()
        {
            MarkedSucessful = true;
            return Task.CompletedTask;
        }

        public bool MarkedSucessful { get; set; }

        public Task MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
            return Task.CompletedTask;
        }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public DateTime? DelayedTo { get; set; }

        public Task MoveToErrors(ErrorReport report)
        {
            Errors.Add(report);
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            Requeued = true;
            return Task.CompletedTask;
        }

        public bool Requeued { get; set; }

        public Task Send(Envelope envelope)
        {
            Sent.Add(envelope);
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = true;
        public string TransportScheme { get; }
    }

    public class Receiver
    {
        private readonly IHandlerPipeline _pipeline;
        private readonly Uri _address;

        public Receiver(IHandlerPipeline pipeline, Uri address)
        {
            _pipeline = pipeline;
            _address = address;
        }

        public Task Receive(byte[] data, IDictionary<string, string> headers, IMessageCallback callback)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var envelope = new Envelope(data, headers, callback)
            {
                ReceivedAt = _address
            };

            envelope.ContentType = envelope.ContentType ?? "application/json";

            return _pipeline.Invoke(envelope);
        }

        public Task Receive(object message, IDictionary<string, string> headers, IMessageCallback callback)
        {
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (headers == null) throw new ArgumentNullException(nameof(headers));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            var envelope = new Envelope(headers)
            {
                Message = message,
                Callback = callback
            };

            envelope.ContentType = envelope.ContentType ?? "application/json";

            return _pipeline.Invoke(envelope);
        }
    }
}
