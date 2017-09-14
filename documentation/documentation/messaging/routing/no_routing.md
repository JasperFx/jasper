<!--title:"No Route" Behavior-->

By default, Jasper will throw a `NoRoutesException` when it cannot determine any subscribing routes from either
<[linkto:documentation/messaging/routing/static_routing;title=static publishing rules]> or <[linkto:documentation/messaging/routing/subscriptions;title=dynamic subscriptions]>.

You can override that behavior to ignore this case and just allow Jasper to log that a message could not be delivered:

<[sample:IgnoreNoRoutes]>

See <[linkto:documentation/messaging/logging]> for more information on logging the "No Routes" event.