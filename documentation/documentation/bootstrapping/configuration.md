<!--title: Application Configuration and Settings-->

<[info]> 
All of the code snippets shown in this topic apply to the JasperRegistry syntax
<[/info]>

Because Jasper applications are also ASP.Net Core applications, the built in [ASP.Net Core configuration just works](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.2), and that is configured the way
you're probably already used to using with the `IWebHostBuilder` model.

On top of that, Jasper supports a form of strong typed configuration
we call the ["Settings" model that was originally used in FubuMVC](https://jeremydmiller.com/2014/11/07/strong_typed_configuration/). This is a lighterweight alternative to ASP.Net Core's `IOptions` model (which is also usable with Jasper).

## Settings Quick Start

Probably the most common scenario is to have a single configuration file mapped to a single object:

1. Add a class that ends with `Settings` to your project, e.g. `MySettings.cs`.
2. Add a json file that has properties that match your `Settings` class.
3. Use the `Build` method to tell Jasper about your configuration file.
4. Include your `Settings` class in the constructor of a class and Jasper will automatically inject the settings object

<[sample:inject-settings]>

## Configuration Lifecycle

Application configuration can come from a mix of the built in .Net Core configuration sources and programmatic options set in either your
`JasperRegistry` or a loaded extension. While you make all the declarations in your `JasperRegistry` class, Jasper takes some steps to execute the usage of configuration options at bootstrapping time like so:

1. Loads the default data for known `Settings` types by looking first for a configuration section named with the prefix of your `Settings` type name.
1. Apply all the `JasperRegistry.Settings.Alter()` or `Replace()` delegates from registered extensions in the order that they were registered
1. Apply all the `JasperRegistry.Settings.Alter()` or `Replace()` delegates configured in your `JasperRegistry` in the order that they were
   registered to ensure that the application specific options always win out over the base options or options coming from an extension



## Alter the Settings Objects with IConfiguration

If you're already using an ASP.Net Core `Startup` class for configuring your application, the easiest way to configure
Jasper `Settings` objects based on that configuration is to just inject that class into your `Startup` and work directly against it as
shown in this example:

<[sample:UsingStartupForConfigurationOfSettings]>

Otherwise, you can do this as well within your `JasperRegistry` class that defines
your application like this simple example that just plucks a value from configuration and applies
that to the `ServiceName` for the application:

<[sample:UsingConfigApp]>



## Modify Settings

It may be necessary to modify a settings object after it has been loaded from configuration.  Settings can be altered:

<[sample:alter-settings]>

or completely replaced:

<[sample:replace-settings]>
