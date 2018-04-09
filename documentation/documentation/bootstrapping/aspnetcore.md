<!--title:Adding Jasper to an ASP.Net Core Application-->

<[info]>
This functionality is very likely to change as it gets used more often and should be considered very preliminary
<[/info]>

.Net is Microsoft's world, and the rest of us are just living in it. Unlike its predecessor [FubuMVC](http://fubumvc.github.io), Jasper tries to
play nice as a citizen in the greater ASP.Net Core ecosystem. To that end, you can use Jasper within an ASP.Net Core application as
just a service bus or command executor or as just another part of the ASP.Net Core runtime pipeline.


## If ASP.Net Core is in charge...

<[warning]>
At the moment, the UseJasper() method has to be the very last declaration against IWebHostBuilder for the timing of actions to work correctly. We think that we'll be able to address this with some changes to hosting that are coming in ASP.Net Core 2.0</div>
<[/warning]>

First off, let's say that you just want to use Jasper as messaging infrastructure inside of an ASP.Net Core application. With that in mind, let's say that you have a `JasperRegistry` like this:

<[sample:SimpleJasperBusApp]>

If you prefer to stick with the idiomatic ASP.Net Core bootstrapping, you can add Jasper to the
mix with the `UseJasper()` extension method as shown below:

<[sample:adding-jasper-to-aspnetcore-app]>


There's a couple things going on in the sample above:

1. Jasper is quietly replacing the ASP.Net Core IoC container with Jasper's internal [Lamar](https://github.com/jasperfx/lamar) container after pulling in all the service registrations made directly to the `IWebHostBuilder`. For the moment, we're doing this under the belief that it's best to avoid having two different IoC containers at runtime.
1. If you don't specify exactly where you want Jasper to run within the middleware pipeline of your ASP.Net Core application, Jasper puts itself as the very last `RequestDelegate` in the middleware chain.
1. The `UseJasper()` method is bootstrapping the `JasperRuntime` and putting that into the IoC container, so that disposing the `IWebHost` will also shut down the Jasper services as well.

To control the order of where Jasper executes within your ASP.Net Core application pipeline, you can
explicitly add the Jasper middleware like this:

<[sample:ordering-middleware-with-jasper]>

## If Jasper is in charge...

<div class="alert alert-success">Doing the bootstrapping the idiomatic Jasper way allows Jasper to parallelize quite a bit of the bootstrapping behavior for faster startup times.</div>

If you choose to use Jasper idiomatically, it can bootstrap ASP.Net Core itself if you're using the HTTP features. `JasperRegistry.Hosting` exposes the ASP.Net `IWebHostBuilder` to configure the Http hosting as shown below:

<[sample:ConfiguringAspNetCoreWithinJasperRegistry]>

In the case above, Jasper will automatically configure itself as the `RequestDelegate` for the application.

To get more precise with how ASP.Net Core middleware is ordered with Jasper, you can again use the
`AddJasper()` extension method:

<[sample:AppWithMiddleware]>

