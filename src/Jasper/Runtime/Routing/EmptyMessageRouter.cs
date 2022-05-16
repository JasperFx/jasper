using System;

namespace Jasper.Runtime.Routing;

internal class EmptyMessageRouter<T> : MessageRouterBase<T>
{
    public EmptyMessageRouter(JasperRuntime runtime) : base(runtime)
    {
    }

    public override Envelope[] RouteForSend(T message, DeliveryOptions? options)
    {
        throw new NoRoutesException(typeof(T));
    }

    public override Envelope[] RouteForPublish(T message, DeliveryOptions? options)
    {
        return Array.Empty<Envelope>();
    }
}