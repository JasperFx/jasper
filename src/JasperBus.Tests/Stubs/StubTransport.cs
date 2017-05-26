﻿using System;
using System.Collections.Generic;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using System.Threading.Tasks;

namespace JasperBus.Tests.Stubs
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

        public Uri ActualUriFor(ChannelNode node)
        {
            return (node.Uri.AbsoluteUri + "/actual").ToUri();
        }

        public void ReceiveAt(ChannelNode node, IReceiver receiver)
        {
            Channels[node.Uri].StartReceiving(receiver);
        }

        public Uri CorrectedAddressFor(Uri address)
        {
            return address;
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
        }

        public Uri DefaultReplyUri()
        {
            return "stub://replies".ToUri();
        }
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

        public void StartReceiving(IReceiver receiver)
        {
            Receiver = receiver;
        }

        public IReceiver Receiver { get; set; }

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

        public void MarkSuccessful()
        {
            MarkedSucessful = true;
        }

        public bool MarkedSucessful { get; set; }

        public void MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
        }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public Task MoveToDelayedUntil(DateTime time)
        {
            DelayedTo = time;
            return Task.CompletedTask;
        }

        public DateTime? DelayedTo { get; set; }

        public void MoveToErrors(ErrorReport report)
        {
            Errors.Add(report);
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
    }
}
