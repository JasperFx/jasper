<!--title: HTTP Services -->

Jasper's high performance command execution pipeline can also be used to service HTTP requests. Jasper's HTTP support can be used as:

1. A complete alternative to MVC Core for Web API development
1. A complement to MVC Core because it's perfectly possible to mix and match ASP.Net Core middleware
1. Maybe odd to consider, but Jasper's <[linkto:documentation/http/mvcextender]> extension can be used to run MVC Core `Controller` 
   endpoints with Jasper's more efficient runtime pipeline and/or to allow Jasper to use common MVC Core constructs like `IActionResult`
1. A more performant replacement for the common ASP.Net MVC + [MediatR](https://github.com/jbogard/MediatR) combination

As a quick start, here's what "Hello, World" looks like in a Jasper HTTP endpoint:

<[sample:SampleHomeEndpoint]>

The endpoint above would be called in the "GET: /" route and would write out the text returned in the method to the response with the 
`content-type` header set to `text/plain`. 

Pretty exciting, right? Not really? Okay, so here's an example of building an HTTP endpoint that returns JSON with a routing argument:

<[sample:simple-json-endpoint]>

And now let's do a POST:

<[sample:simple-json-post]>

Hopefully you've already noticed a couple things about Jasper:

* It's dependent upon naming conventions for the route patterns (but this can be overridden explicitly)
* There are no required attributes, marker interfaces, `ControllerBase` abstract types or generally any other kind of framework cruft that
  is so prevalent in .Net frameworks
* The route handlers are just .Net methods
* Jasper route handlers can be static or instance methods and either asynchronous or synchronous whenever that's acceptable

Behind the scenes, Jasper is using Lamar to do some runtime code generation and compilation to make the actual HTTP `RouteHandler` as efficient as possible.

Here's quite a bit more about the HTTP support:

<[TableOfContents]>