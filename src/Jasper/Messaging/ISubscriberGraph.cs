using System;

namespace Jasper.Messaging
{

    public interface ISubscriberGraph
    {
        ISubscriber GetOrBuild(Uri address);

        bool HasSubscriber(Uri uri);

        ISubscriber[] AllKnown();

    }
}
