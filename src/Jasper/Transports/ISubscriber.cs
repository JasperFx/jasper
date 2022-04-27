using System;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Transports.Sending;

namespace Jasper.Transports
{
    public interface ISubscriber
    {
        bool ShouldSendMessage(Type messageType);

        void AddRoute(MessageTypeRouting routing, IJasperRuntime runtime);
    }
}
