<!--title:Bootstrapping & Configuration-->

<[info]>
Jasper uses the ASP.Net Core `IWebHostBuilder` infrastructure internally for bootstrapping now, even for idiomatic Jasper
bootstrapping.
<[/info]>

All the examples in this page are using the default, "in the box" options for Jasper. To see what else can be configured or added to a Jasper application, see the folling topics:

<[TableOfContents]>

Even if running "headless" (i.e., without Kestrel), Jasper applications are effectively ASP.Net Core applications and use the ASP.Net Core `IWebHostBuilder` for all bootstrapping and application lifecycle events. 

In its simplest possible setup, you can fire up a Jasper application in memory like so:

<[sample:simplest-aspnetcore-bootstrapping]>

More likely though, you'll want to run a Jasper-ized ASP.net Core application from a command line application. Jasper goes all in on command line tooling with quite a bit of its own diagnostics, so naturally it comes with a first class citizen for bootstrapping and executing from the command line
with `JasperHost` like so:

<[sample:simplest-aspnetcore-run-from-command-line]>

Or with the usage of an extension method in Jasper, this is an exact equivalent:

<[sample:simplest-aspnetcore-run-from-command-line-2]>






## Headless Applications

If you are building a Jasper application that does not expose any HTTP endpoints or needs to customize the underlying `IWebHostBuilder`, you can use
`JasperHost.CreateDefaultBuilder()` as shown below to create a pre-configured `IWebHostBuilder` that is lighter than `WebHost.CreateDefaultBuilder()` that you would use for HTTP projects:

<[sample:Bootstrapping-Basic2]>

This default `IWebHostBuilder` behind the scenes is this:

<[sample:default-configuration-options]>


There is also a shortcut for bootstrapping directly with `JasperHost` like this:

<[sample:Bootstrapping-Basic]>

Which is just syntactical sugar for:

<[sample:Bootstrapping-Basic2]>

And likewise, to run a Jasper application from a command line application, you can again use `JasperRuntime` like so:

<[sample:simplest-idiomatic-command-line]>

This option might be enough to do some useful things with Jasper as a command executor at the least, but more likely you'll want to add other elements to your system like additional services to the <[linkto:documentation/ioc;title=underlying IoC container]>, <[linkto:documentation/messaging/configuration;title=the messaging configuration]>, or <[linkto:documentation/bootstrapping/aspnetcore;title=ASP.Net Core middleware]>.

All configuration and set up of Jasper starts with the `JasperRegistry` class. Typically you would subclass `JasperRegistry`, but if you have only minimal configuration needs, you might bootstrap like this:

<[sample:Bootstrapping-Basic3]>

More likely though is that you will opt to define your application with a custom `JasperRegistry`:

<[sample:CustomJasperRegistry]>

And then use that to bootstrap your application:

<[sample:Bootstrapping-with-custom-JasperRegistry]>



