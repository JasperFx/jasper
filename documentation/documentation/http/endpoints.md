<!--title:Routing and Endpoint Actions-->

<[info]>
See also the <[linkto:documentation/http/mvcextender]> for more information about using the ASP.Net Core routing attributes as
an alternative to using Jasper's built in Url conventions and discovery.
<[/info]>

Jasper handles HTTP routes by "knowing" which Url patterns map to methods on .Net classes along with some knowledge about how to deal with any route
arguments. 


## Route Discovery

Jasper uses a naming convention to scan through your application assembly (the assembly that holds either your ASP.Net Core `Startup` class or a `JasperRegistry` type) to find public methods that handle HTTP requests. Out of the box, the naming convention is:

* Public types that are named with the suffix "Endpoint" or "Endpoints" and public method names that start with an HTTP verb and an underscore, like
  "get_something" or "post_something" or public methods decorated with one of Jasper's routing attributes like `[JasperGet]` or `[JasperPost]`
* Public classes named either `HomeEndpoint` or `ServiceEndpoint`, and methods named after HTTP verbs like `Get()` or `Delete()` or `Head()`

Endpoint classes can be static classes if you want to only lean on method injection, and that's a minor performance optimization because it reduces
object allocations at runtime.



## Root Urls

Jasper has some special naming conventions for the root Urls of your application ("GET: /", "POST: /", etc.). Using either the class name `HomeEndpoint` (the old FubuMVC standard) or `ServiceEndpoint`, use methods named after HTTP verbs like:

<[sample:ServiceEndpoint]>

The `Index()` method is also considered to mean "GET: /" as a holdover from older frameworks.


## Url Patterns

<[info]>
Jasper's routing assumes *for the moment* that a single route only applies to a single HTTP method ("GET", "POST", etc.)
<[/info]>

Jasper's naming convention (inherited from FubuMVC) is to look for methods that follow the pattern `http method_segment1_segment2_segment3`. Jasper determines the actual route path by:

1. Split the method name by underscore character and assuming that the resulting array is the segments in the route pattern
1. Use the first segment as the HTTP verb 
1. Look for any segments that exactly match the name of an argument parameter to the action method. If so, this segment is assumed to be a route argument

Do see the section on "Spread" routes below for a discussion about these special kinds of routes.

## Using Attributes

As a late addition just prior to v1.0, Jasper also supports using attributes on methods to explicitly denote the Url pattern as shown below:

<[sample:AttributeUsingEndpoint]>

In addition, if you can also pull in the <[linkto:documentation/http/mvcextender]> to use MVC Core's built in routing attributes as another alternative.


## Using Underscores and Dashes in Route Patterns

Because somebody is going to ask, yes, it's possible to use underscores or dashes in your Url patterns. To prefix a route segment
with an underscore, use 2 underscores. For a dash in any location in the Url pattern, use 3 underscores. Here's a sample:

<[sample:using-dash-and-underscore-in-routes]>

If you need more control than this, you can always use the `[HttpGet()]` type routing attributes with <[linkto:documentation/http/mvcextender]>.

## Route Arguments

Jasper supports the concept of route arguments for the following types (so far):

1. `Guid`
1. Any .Net number type
1. Strings
1. `DateTime` using the [ISO 8601 format](https://en.wikipedia.org/wiki/ISO_8601)
1. `char`
1. `bool`

The naming convention inside of Jasper is to find route segments that exactly match the parameter name of an argument to
the HTTP action. So in this example:

<[sample:using-guid-route-argument]>

The Url route pattern is "GET: /guid/:id" with the *:id* segment being a route argument that at runtime is converted to a `Guid` by parsing that part of the Url and passing that value into the `id` parameter of the `get_guid_id` method.

You can also have multiple route arguments and even separate the arguments with just plain text segments as shown in this sample:

<[sample:using-multiple-arguments]>

which ends up being the route `GET: /letters/:first/to/:end`. You can see this route in action with the following test (using [Alba](https://jasperfx.github.io/alba)):

<[sample:using-char-arguments]>


## "Spread" / "Resource" / "Path" arguments

Occasionally, what you really need is a route that let's you capture a path with a variable number of route segments. Jasper let's you do this 
with what it calls a *Spread* route (named after the Javascript spread operator even though it's not really similar). Jasper has two mechanisms 
for spread route. The first is to use a parameter argument named `resourcePath` as shown below:

<[sample:SpreadHttpActions-by-path]>

If you were running a Jasper application with the endpoint above, and called the app with the Url "/folder/one/two/three", Jasper would call 
the `get_folder` method and pass in the string "one/two/three" to the `resourcePath` argument.

Likewise, you can also expose a method parameter named `pathSegments` of type `string[]` as shown below:

<[sample:SpreadHttpActions-by-segments]>

In the case above, if you called the Url "/file/one/two/three", Jasper would call the `get_file` method and pass a string array 
of `string[]{"one", "two", "three"}` into the `pathSegments` argument.


## Synchronous vs. Asynchronous Endpoint Actions

Jasper allows you to write your action methods as either synchronous or asynchronous methods. We know that we should be using asynchronous
methods any time we're doing any kind of system IO or accessing another system, but once in awhile there are operations that just don't
require us to be asynchronous. Jasper is agnostic, and will happily wrap the right calling code around any kind of method.

Just as in MVC Core, if an action method returns `Task<T>`, Jasper assumes that the real resource type returned by the method is `T`.


## Request Bodies

Jasper's convention is that the first argument to an endpoint action is the message body *unless it's a route argument*.

In this code shown below, the `input` parameter is assumed to be the message body:

<[sample:NumbersEndpoint]>

## Method and Constructor Injection

If an endpoint class is not a static class, objects of your endpoint class will be created at runtime (by generated code in most cases) and it's 
possible to use [constructor injection](https://en.wikipedia.org/wiki/Dependency_injection) to pull in dependencies of your endpoint like this sample
that injects a [Marten IQuerySession](https://jasperfx.github.io/marten) and an `ILogger` as constructor dependencies:

<[sample:MartenUsingEndpoint-with-ctor-injection]>

Likewise, you can also take advantage of *method injection* in Jasper where you simply allow Jasper to pass in the service
dependencies through method arguments. Using method injection, the code above becomes this:

<[sample:MartenStaticEndpoint]>

You can also mix and match the two approaches, but the Jasper team probably argues for some consistency at least at the class level if 
not throughout the codebase.

## Access to the HttpContext for the Request

The `HttpContext` object for the current request can be passed into your methods as an argument:

<[sample:injecting-httpcontext]>

As a convenience, you can also get at the `HttpRequest` or `HttpResponse` independently too:

<[sample:injecting-request-and-response]>

Beyond this, you can inject any other service by type that is a public property of the ASP.Net Core `HttpContext` type.


