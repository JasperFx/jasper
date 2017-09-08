using System;
using System.Collections.Generic;

namespace Jasper.Bus
{

    public interface IChannelGraph : IEnumerable<IChannel>
    {
        IChannel this[Uri uri] { get; }

        IChannel DefaultChannel { get; }
        IChannel DefaultRetryChannel { get; }

        string Name { get; }

        string[] ValidTransports { get;}
        IChannel TryGetChannel(Uri address);
        bool HasChannel(Uri uri);
    }
}
