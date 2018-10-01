using System;

namespace Jasper.Messaging.Runtime.Routing
{
    [Obsolete("Going away soon")]
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }
}
