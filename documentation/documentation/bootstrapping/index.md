<!--title:Bootstrapping-->

To configure and bootstrap a Jasper application, you are primarily interacting with just a handful of types:

1. `JasperRuntime` - this manages the lifecycle of a Jasper application from bootstrapping to cleanly shutting down the application and releasing resources. It also exposes the underlying IoC container for the application and several members that just provide information about the running application
1. `JasperRegistry` - this class is used to configure all the options of a Jasper application
1. `JasperAgent` - a static helper for bootstrapping and managing a `JasperRuntime` from
a console application. See <[linkto:documentation/bootstrapping/console]> for more information
1. `IWebHostBuilder.UseJasper()` or `IApplicationBuilder.AddJasper()` - extension methods provided to add Jasper to an ASP.Net Core application. See <[linkto:documentation/bootstrapping/aspnetcore]> for more information.

If you do not need to override or add to any of Jasper's default configuration, you can happily bootstrap a `JasperRuntime` like this:

<[sample:Bootstrapping-Basic]>

Which is just syntactical sugar for:

<[sample:Bootstrapping-Basic2]>

This option might be enough to do some useful things with Jasper as a command executor at the least, but more likely you'll want to add other elements to your system like additional services to the <[linkto:documentation/ioc;title=underlying IoC container]>, <[linkto:documentation/messaging/channels;title=messaging channels]>, or <[linkto:documentation/bootstrapping/aspnetcore;title=ASP.Net Core middleware]>.

All configuration and set up of Jasper starts with the `JasperRegistry` class. Typically you would subclass `JasperRegistry`, but if you have only minimal configuration needs, you might bootstrap like this:

<[sample:Bootstrapping-Basic3]>

More likely though is that you will opt to define your application with a custom `JasperRegistry`:

<[sample:CustomJasperRegistry]>

And then use that to bootstrap your application:

<[sample:Bootstrapping-with-custom-JasperRegistry]>

See also:

<[TableOfContents]>

