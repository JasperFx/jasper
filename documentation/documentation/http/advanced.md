<!--title:Advanced HTTP Configuration-->

This topic is a grab bag of rarely used options, but stuff you may want to exploit for more advanced scenarios. Honestly, if you find yourself
wanting to use something here, ask any questions you might have in the Jasper Gitter room as linked in the header above.


## "Fast" Mode

ASP.Net Core has middleware applied automatically to open a scoped IoC container for an HTTP request before any other middleware is executed, such
that every bit of middleware and MVC Core itself can use a single IoC request scope all the way through that request. Great, but that's generally 
unnecessary for Jasper actions and maybe just a waste of server resources. To go a little faster in Jasper, we've got a setting that will reach into your ASP.Net Core configuration and rip that silly thing out:

<[sample:GoFasterMode]>

## Disable HTTP Routes Altogether

Meh, maybe you don't want the Jasper routing to be in effect whatsoever and only use Jasper for its messaging or command execution. In that case, you 
can disable all route discovery like so:

<[sample:NoHttpRoutesApp]>

or with `IWebHostBuilder` as the bootstrapping:

<[sample:NoHttpRoutesInWebHoster]>


## Policies

To apply middleware to Jasper routes in a conventional way, you can use the `IRoutePolicy` interface like this:

<[sample:IRoutePolicy]>

As an example, the <[linkto:documentation/http/mvcextender]> library has a custom policy to add some Jasper middleware
around endpoint actions implemented by `ControllerBase` classes for some MVC Core compliant runtime actions:

<[sample:ControllerUsagePolicy]>

To actually apply a route policy, you can add it to the `JasperOptionsBuilder.HttpRoutes.GlobalPolicy()` method as shown below:

<[sample:applying-route-policy]>

## Reader/Write Rules

Maybe just for Jasper development itself, but there's an extension point that will allow you to customize how Jasper generates code inside of its `RouteHandler` classes for certain input types or resource types. Here's the built in writer rule for `string` resource types:

<[sample:WriteText]>

To register the custom rules, you can register `IWriterRule` or `IReaderRule` services with the IoC service registrations.


## Ignore Endpoint or Controller Actions

You can mark any type or method with the `[JasperIgnore]` attribute that tells Jasper not to use that whole class or individual method as an HTTP action method.

