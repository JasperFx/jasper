using System;

namespace JasperBus.Runtime.Routing
{
    public interface IRoutingRule
    {
        bool Matches(Type type);
    }
}