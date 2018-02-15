using System;

namespace Jasper.Messaging
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
