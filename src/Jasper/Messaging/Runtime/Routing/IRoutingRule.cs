using System;

namespace Jasper.Messaging.Runtime.Routing
{
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }
}