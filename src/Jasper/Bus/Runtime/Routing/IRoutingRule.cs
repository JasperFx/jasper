using System;

namespace Jasper.Bus.Runtime.Routing
{
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }
}