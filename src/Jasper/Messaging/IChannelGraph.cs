using System;

namespace Jasper.Messaging
{

    public interface IChannelGraph
    {
        string[] ValidTransports { get;}

        IChannel GetOrBuildChannel(Uri address);

        bool HasChannel(Uri uri);

        IChannel[] AllKnownChannels();

        Uri SystemReplyUri { get; }
    }
}
