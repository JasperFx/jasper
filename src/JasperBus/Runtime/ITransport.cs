using System;
using System.Collections.Generic;
using JasperBus.Configuration;

namespace JasperBus.Runtime
{
    public interface ITransport : IDisposable
    {
        string Protocol { get; }

        Uri ReplyUriFor(ChannelNode node);

        void Send(Uri uri, byte[] data, Dictionary<string, string> headers);

        Uri ActualUriFor(ChannelNode node);

        void ReceiveAt(ChannelNode node, IReceiver receiver);
    }
}