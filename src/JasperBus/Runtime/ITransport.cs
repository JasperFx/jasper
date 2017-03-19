using System;
using System.Collections.Generic;
using JasperBus.Configuration;
using JasperBus.Runtime.Invocation;

namespace JasperBus.Runtime
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Uri ReplyUriFor(Uri node);

        void Send(Uri uri, byte[] data, Dictionary<string, string> headers);

        Uri ActualUriFor(ChannelNode node);

        Uri CorrectedAddressFor(Uri address);

        void StartReceiving(IHandlerPipeline pipeline, ChannelGraph channels);
    }
}