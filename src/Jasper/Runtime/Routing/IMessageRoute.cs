using System;

namespace Jasper.Runtime.Routing;

public interface IMessageRoute
{
    Uri Destination { get; }

    string ContentType { get; }
    void Configure(Envelope envelope);
    Envelope CloneForSending(Envelope envelope);

    // TODO -- WATCH THIS!!!! Need to use a consistent Id!
    Envelope BuildForSending(object message);
}
