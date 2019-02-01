<!--title:Configuring Jasper Applications-->

While this topic dives into some of the more general options available in `JasperRegistry`, check out these topics for deeper
discussions of setting up Jasper applications:

* <[linkto:documentation/bootstrapping/configuration]> for integrating with [Configuration in .Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) and Jasper's [strong-typed configuration](https://jeremydmiller.com/2014/11/07/strong_typed_configuration/) *Settings* model
* <[linkto:documentation/bootstrapping/aspnetcore]> to make Jasper act as just a citizen in the greater ASP.Net Core ecosystem
* <[linkto:documentation/http]> for information on configuring ASP.Net Core middleware and customizing Jasper's HTTP service support
* <[linkto:documentation/messaging]> for setting up messaging receivers, subscriptions, and publishing in your application

## Service Name

By default, Jasper derives a descriptive _ServiceName_ for your application by taking the class name of your custom `JasperRegistry` and stripping off
any "JasperRegistry" or "Registry" suffix. If you are not using a custom `JasperRegistry`, Jasper will use the main application assembly name as the service name. For diagnostic purposes you may want to override the service name like so:

<[sample:CustomServiceRegistry]>

## Service Registrations

<div class="alert alert-info"><b>Note!</b> Jasper was conceived and designed in no small part to reduce the role of an IoC container at runtime, but "much, much less" is still more than "none." </div>

Like most application frameworks in .Net, Jasper uses an IoC container to do basic composition within its runtime pipeline. You can add your own registrations to the application container directly in any combination of:

1. The `Startup.ConfigureServices()` method. Use this mechanism if you need to do conditional registrations based on the hosting environment. I.e., apply this service if in "Testing" or this other one if in "Development."
1. Using the `IWebHostBuilder.ConfigureServices()` extension method
1. With `JasperRegistry.Services` for [Lamar-centric](https://jasperfx.github.io/lamar) registrations (which also supports the ASP.Net Core service registrations as well).

But, the Jasper team **strongly recommends that you make all your service registrations in one place as much as possible** for easier debugging later.

Using your `JasperRegistry`:

<[sample:Bootstrapping-ServiceRegistrations]>

See <[linkto:documentation/bootstrapping/ioc]> for a lot more information about how Jasper uses an IoC container because it is a little bit different than other .Net frameworks.

## Adding Extensions

Jasper comes with its own extensibility model based on an interface called `IJasperExtension`. A custom extension
might look something like this:

<[sample:Bootstrapping-CustomJasperExtension]>

The syntax in `JasperRegistry` to apply that extension is shown below:

<[sample:AppWithExtensions]>

See <[linkto:documentation/extensions]> for more information on building, using, and auto-discovering Jasper extensions.







