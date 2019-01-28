<!--title:Reverse Url Lookup-->

Sometimes you may want to interrogate Jasper and look up the Url that would match an Http endpoint action method or an input model type. This frequently arises using full Hypermedia within ReSTful services and also in server side rendered views. To that end, there is a service in your IoC
container called `IUrlRegistry` in Jasper applications that can be used to look up the Url for a given endpoint action or model type. Unlike MVC Core, this lookup works against a "known" model of routes to endpoint action methods and is not dependent upon *how* the route was configured.

## Look up by Input Type

If you have the model type or an object, and want to know what the Url would be for the action that accepts that type as its request body, use one of these methods:

<[sample:LookupByInputType]>

It's unnecessary to specify the HTTP method if there is only one route that matches the input type.


## Look up by Endpoint Method

If a Url has no route arguments, you can easily find the Url by the endpoint handler type and method as these examples demonstrate:

<[sample:LookupByMethod]>

If you have route arguments, you're still in luck. In this case, use the lookup by `Expression` and specify the route arguments. Say you have this HTTP action method:

<[sample:get_range_from_to]>

To look up the Url for this method with route arguments, you pass those arguments through the `Expression` argument like this:

<[sample:doing-url-lookup-with-route-arguments]>




## Named Routes

Finally, as you may have noticed in the sample with the `get_range_from_to()` method above, Jasper also lets you tag endpoint actions with a route name using Jasper's `[RouteName]` attribute. That helps short circuit the Url lookup to this usage:

<[sample:url_for_named_route_with_arguments]>