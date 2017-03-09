using System;
using System.Collections.Generic;

namespace JasperBus.Runtime
{
    public interface IChannel : IDisposable
    {
        Uri Address { get; }
        void StartReceiving(IReceiver receiver);
        void Send(byte[] data, Dictionary<string, string> headers);
    }
}