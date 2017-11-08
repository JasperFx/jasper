using System;
using System.Collections.Generic;

namespace Jasper.Bus
{

    public interface IChannelGraph
    {
        IChannel DefaultChannel { get; }

        string[] ValidTransports { get;}

        IChannel GetOrBuildChannel(Uri address);

        bool HasChannel(Uri uri);

        IChannel[] AllKnownChannels();

        Uri SystemReplyUri { get; }
    }
}
