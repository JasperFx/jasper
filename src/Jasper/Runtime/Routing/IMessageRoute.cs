using System;
using System.Collections.Generic;

namespace Jasper.Runtime.Routing
{
    public interface IMessageRoute
    {
        void Configure(Envelope envelope);
        Envelope CloneForSending(Envelope envelope);

        // TODO -- WATCH THIS!!!! Need to use a consistent Id!
        Envelope BuildForSending(object message);

        Uri Destination { get; }

        string ContentType { get; }
    }
}
