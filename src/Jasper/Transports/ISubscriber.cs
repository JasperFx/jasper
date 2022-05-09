using System;
using Jasper.Runtime;
using Jasper.Runtime.Routing;

namespace Jasper.Transports;

public interface ISubscriber
{
    bool ShouldSendMessage(Type messageType);

    void AddRoute(MessageTypeRouting routing, IJasperRuntime runtime);
}
