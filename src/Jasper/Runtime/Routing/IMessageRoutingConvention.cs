using System;
using System.Collections.Generic;
using Jasper.Configuration;

namespace Jasper.Runtime.Routing;

/// <summary>
/// Plugin for doing any kind of conventional message routing
/// </summary>
public interface IMessageRoutingConvention
{
    void DiscoverListeners(IJasperRuntime runtime, IReadOnlyList<Type> handledMessageTypes);
    IEnumerable<Endpoint> DiscoverSenders(Type messageType, IJasperRuntime runtime);
}
