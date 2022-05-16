using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.Transports.Local;

namespace Jasper.Runtime.Routing;

internal class MultipleRouteMessageRouter<T> : MessageRouterBase<T>
{
    private readonly MessageRoute[] _routes;

    public MultipleRouteMessageRouter(JasperRuntime runtime, IEnumerable<MessageRoute> routes) : base(runtime)
    {
        _routes = routes.ToArray();

        foreach (var route in _routes.Where(x => x.Sender.Endpoint is LocalQueueSettings))
        {
            route.Rules.Fill(HandlerRules);
        }

    }

    public override Envelope[] RouteForSend(T message, DeliveryOptions? options)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        return RouteForPublish(message, options);
    }

    public override Envelope[] RouteForPublish(T message, DeliveryOptions? options)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        var envelopes = new Envelope[_routes.Length];
        for (int i = 0; i < envelopes.Length; i++)
        {
            envelopes[i] = _routes[i].CreateForSending(message, options, LocalDurableQueue, Runtime);
        }

        return envelopes;
    }
}
