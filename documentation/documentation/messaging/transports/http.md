<!--title:HTTP Transport-->

With the ASP.Net team putting so much effort into making [Kestrel](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/servers/?view=aspnetcore-2.1&tabs=aspnetcore2x) performant and robust, it seemed to make perfect sense
to support an alternative transport based on Kestrel that can be used to send messages between Jasper applications. 

Why not just use HTTP services if you're going to use the HTTP transport described here? Because the transport adds all the support for
retries, [back pressure](https://www.reactivemanifesto.org/glossary#Back-Pressure), durability, and error handling you expect with a 
service bus that you would have to at least partially build in yourself if you were only using HTTP services for integration.

There's a couple things to note about the HTTP transport option:

* *Sending* messages through the HTTP transport is enabled by default in any Jasper application
* *Receiving* messages through the HTTP transport requires you to explicitly enable HTTP message listening as shown in the code sample below. You will also need to explicitly add Kestrel hosting through your application bootstrapping as you normally do in ASP.Net Core 
applications. 
* Enabling the HTTP transport listening adds a pair of new HTTP endpoints to your Jasper application
* The messages sent are batched similar to the <[linkto:./tcp]>
* Likewise, the <[linkto:./durable;title=message persistence]> can be combined with the HTTP transport
* Jasper itself adds the additional routes from Jasper's own HTTP router. See <[linkto:documentation/bootstrapping/aspnetcore]> for 
  more information on ordering Jasper's middleware within your larger ASP.Net Core application


## Configuring the HTTP Transport 

You can enable the HTTP transport listening, the relative url of the message endpoint, and the connection timeout
for sending message batches with the code shown below:

<[sample:AppUsingHttpTransport]>

By default, the messages are received on a pair of routes in your system:

1. PUT: /messages -- receives message batches in a non-durable manner
1. PUT: /messages/durable -- receives message batches and uses the <[linkto:./durable]>

As shown above, you can replace the "messages" segment for the base Url of the messaging receiver.

## Publishing to the HTTP Transport

<[warning]>
Using HTTPS is a little bit of a work in progress. Follow our [GitHub issue](https://github.com/JasperFx/jasper/issues/397) for progress on this. 
<[/warning]>

To configure messages to be published or received through the HTTP transport, you use the Url to the `/messages` or `/messages/durable` endpoint
of the downstream system like this:

<[sample:HttpTransportUsingApp]>

See <[linkto:documentation/messaging/routing]> for more information on configuring publishing rules and subscriptions.


## Configuring Authentication with the HTTP Transport

Yeah, this one is a [work in progress](https://github.com/JasperFx/jasper/issues/398).
