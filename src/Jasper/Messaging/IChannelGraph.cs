using System;

namespace Jasper.Messaging
{

    public interface IChannelGraph
    {
        IChannel GetOrBuildChannel(Uri address);

        bool HasChannel(Uri uri);

        IChannel[] AllKnownChannels();

    }
}
