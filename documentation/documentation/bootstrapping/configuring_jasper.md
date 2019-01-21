<!--title:Configuring Jasper Applications-->

While this topic dives into some of the more general options available in `JasperRegistry`, check out these topics for deeper
discussions of setting up Jasper applications:

* <[linkto:documentation/bootstrapping/configuration]> for integrating with [Configuration in .Net Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) and Jasper's [strong-typed configuration](https://jeremydmiller.com/2014/11/07/strong_typed_configuration/) *Settings* model
* <[linkto:documentation/bootstrapping/aspnetcore]> to make Jasper act as just a citizen in the greater ASP.Net Core ecosystem
* <[linkto:documentation/http]> for information on configuring ASP.Net Core middleware and customizing Jasper's HTTP service support
* <[linkto:documentation/messaging]> for setting up messaging receivers, subscriptions, and publishing in your application

## Service Name and Hosting Environment


By default, Jasper derives a descriptive _ServiceName_ for your application by taking the class name of your `JasperRegistry` and stripping off
any "JasperRegistry" or "Registry" suffix. For diagnostic purposes you may want to override the service name like so:

<[sample:CustomServiceRegistry]>

Jasper exposes the [ASP.Net Core Environment name](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments) with this usage:

<[sample:EnvironmentNameRegistry]>

You can use the `EnvironmentName` property within the constructor function of your `JasperRegistry` to do conditional configuration based on environment just like you would in an ASP.Net Core application:

<[sample:UsingEnvironmentName]>

You can also access the current application directory and other information from the `IHostedEnvironment` property on the context object in both signatures.

If you are using `IWebHostBuilder` for bootstrapping, the Jasper service name and ASP.Net Core environment can be set like this:

<[sample:aspnetcore-idiomatic-option-configuration]>

## Service Registrations

<div class="alert alert-info"><b>Note!</b> Jasper was conceived and designed in no small part to reduce the role of an IoC container at runtime, but "much, much less" is still more than "none." </div>

Like most application frameworks in .Net, Jasper uses an IoC container to do basic composition within its runtime pipeline. You can add your own registrations to the application container directly in any combination of:

1. The `Startup.ConfigureServices()` method
1. Using the `IWebHostBuilder.ConfigureServices()` extension method
1. With `JasperOptionsBuilder.Services` / `JasperRegistry.Services` for [Lamar-centric](https://jasperfx.github.io/lamar) registrations.

But, the Jasper team **strongly recommends that you make all your service registrations in one place** for easier debugging later.

Using your `JasperRegistry`:

<[sample:Bootstrapping-ServiceRegistrations]>

See <[linkto:documentation/ioc]> for a lot more information about how Jasper uses an IoC container because it is a little bit different than other .Net frameworks.

## Adding Extensions

Jasper comes with its own extensibility model based on an interface called `IJasperExtension`. A custom extension
might look something like this:

<[sample:Bootstrapping-CustomJasperExtension]>

The syntax in `JasperRegistry` to apply that extension is shown below:

<[sample:AppWithExtensions]>

See <[linkto:documentation/extensions]> for more information on building, using, and auto-discovering Jasper extensions.







