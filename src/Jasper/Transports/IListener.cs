using System;

namespace Jasper.Transports;

public interface IListener : IChannelCallback, IAsyncDisposable
{
    Uri Address { get; }
}
