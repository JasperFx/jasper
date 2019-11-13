using System;

namespace Jasper.Messaging
{
    public interface ISubscriberGraph
    {
        ISubscriber GetOrBuild(Uri address);

        ISubscriber[] AllKnown();
    }
}
