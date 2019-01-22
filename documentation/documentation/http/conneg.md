<!--title:Content Negotiation, Resources, and Input Body-->

Jasper tries to make it as easy as possible for you to code straight down to the ASP.Net Core metal as needed by just giving you access to `HttpContext` and its children at any point like this valid Jasper endpoint:

<[sample:crude-http]>

The Jasper team thinks that's absolutely necessary for one off cases or any time you just need more power over the HTTP response. *Most* of the time however, we believe that you'll be much more productive using Jasper's support for the ["one model in, one model out"](http://codebetter.com/jeremymiller/2008/10/23/our-opinions-on-the-asp-net-mvc-introducing-the-thunderdome-principle/) approach where you mostly deal with strong typed .Net objects coming in and out, while letting Jasper deal with the repetitive muck of JSON serialization or whatever representation you're using.

For an HTTP endpoint action, Jasper formalizes the concept of an optional *input type* based on the signature of the endpoint action
that will be read from the `HttpRequest.Body` data during an HTTP request -- most commonly by deserializing JSON into a .Net type. Likewise, the return
value in the action signature is termed a "resource type." During the execution of an endpoint action with a resource type, the value returned would be
written into the `HttpResponse.Body` -- most commonly by serializing the object into JSON.

Here's a concrete example showing both synchronous and asychronous methods:

<[sample:ResourceAndInputTypes]>



*Resources* in Jasper are the model objects returned by endpoint actions. In the Jasper execution pipeline, resources are rendered
into the HTTP response pipeline. Consider these two endpoint actions:

<[sample:ResourceEndpoints]>

In both methods, the resource type is the `Invoice` class.


In all cases, the usage of input types and resource types can be mixed with route arguments as shown below:

<[sample:resource-with-argument]>

Input types can be used without any kind of resource model if you are issuing some kind of command to the web service:

<[sample:input-type-without-any-resource-type]>

In the case above, Jasper (well, actually ASP.Net Core itself) will mark successful requests with `HttpResponse.StatusCode = 200`, but otherwise there's no other response. As shown in a section below, you can also take control over the status code by returning an `int` that Jasper will assume is the status code value:

<[sample:input-type-without-any-resource-type-but-a-status-code]>


## String Resources

If an action method returns a .Net `string` object or `Task<string>`, the results of the method will be written to the outgoing HTTP response
with the content type header value `text/plain`. Consider this endpoint method:

<[sample:StringEndpoint]>

The behavior of that method is demonstrated in this test from the Jasper codebase:

<[sample:StringEndpointSpec]>

## Working with Json

Jasper's obvious default rendering strategy for any concrete resource type besides `string` or `int` is to write out the response by serializing the resource to JSON. Jasper uses [Newtonsoft.Json](https://www.newtonsoft.com/json) as its default JSON serializer, but the JSON serialization can be customized.

For example, this endpoint would expect to read the request body as JSON by deserializing the request body to the `SomeNumbers` type, then serialize the outgoing `SumValue` type to the response:

<[sample:NumbersEndpoint]>

To customize the JSON serialization with the built in Newtonsoft.Json serialization, register a custom `JsonSerializationSettings` object with the application's IoC registrations.

You can do that through the `Startup` class you're using to configure ASP.Net Core:

<[sample:overwriting-the-JSON-serialization-with-StartUp]>

Or if you're using idiomatic Jasper style bootstrapping, you could instead use native Lamar service registrations like this:

<[sample:overwriting-the-JSON-serialization-with-JasperRegistry]>



## Status Code

If an action method returns an `int` or `Task<int>`, the default behavior is to return an empty Http response body, but to set the `HttpResponse.StatusCode` property to the result of the method.

For example, the behavior of these endpoint methods below:

<[sample:StatusCodeEndpoint]>

is demonstrated by this test:

<[sample:StatusCodeEndpointSpec]>


## Content Negotiation

<[info]>
The reader/writer support and even the content negotiation is shared between the messaging and HTTP support within Jasper
<[/info]>

Jasper also supports the concept of [content negotiation](https://en.wikipedia.org/wiki/Content_negotiation). If you need to support multiple representations or formats of either the input or resource type for an endpoint action, you can register any number of custom readers and writers with Jasper, and Jasper will utilize content negotiation at runtime to choose the proper readers and writers based on the `content-type` and `accepts` headers in the HTTP request. If the request does not match a valid reader as specified in the request's `content-type` header, Jasper will abort the request and return a `415` status code for *Unsupported Media Type*. Likewise, if the `accepts` header in the request does not match any of the known writers for the endpoint, Jasper will abort the request with a `406` status code meaning *Not Acceptable*.

For complete examples of using content negotiation in Jasper, check out [the acceptance tests in the codebase for conneg](https://github.com/JasperFx/jasper/blob/master/src/Jasper.Testing/Http/ContentHandling/content_negotiation.cs).

For custom resource representations, you need to implement Jasper's `IMessageSerializer` interface like so:

<[sample:IMediaWriter]>

To help speed things along, there's a base class called `Jasper.Conneg.MessageSerializerBase<T>` that does some of the common grunt work with reading and writing header values.

A custom writer that writes an Xml representation of the `Invoice` resource type would look something like this below:

<[sample:InvoiceXmlWriter]>

Likewise, for custom readers of the input type, use the `IMessageDeserializer` interface:

<[sample:IMediaReader]>

And there is a `Jasper.Conneg.MessageDeserializerBase<T>` base class that does some of the repetitive work for you.

A custom reader that reads an Xml representation of the `Invoice` model as an input type would look something like this below:

<[sample:InvoiceXmlReader]>

Jasper will automatically find and register any `IMessageSerializer` or `IMessageDeserializer` types in your main application assembly. Otherwise, you can register these objects directly into your application's underlying IoC container like so:

<[sample:registering-custom-readers-writers]>




