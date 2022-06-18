<!--title:Bootstrapping & Configuration-->

As of the 1.0 release, Jasper plays entirely within the existing .NET Core ecosystem and depends on the [generic hosting](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1) released as part of .NET Core 3.0 (`IHostBuilder`) for bootstrapping.

All Jasper configuration starts with the [JasperOptions](https://github.com/JasperFx/jasper/blob/master/src/Jasper/JasperOptions.cs) class and the `UseJasper()` extension method that hangs off of `IHostBuilder`.

Say that you are starting with the `dotnet new worker` template to build a headless Jasper
application (i.e., no HTTP endpoints and no user interface of any kind). After adding a reference to the Jasper nuget,
the `Program` class would look like this:

snippet: sample_SimpleJasperWorker

Do be aware that Jasper can **only function with [Lamar](https://jasperfx.github.io/lamar) as the underlying IoC container** and
the call to `UseJasper()` quietly replaces the built in ASP.NET Core DI container with Lamar.

See <[linkto:documentation/ioc]> for more information.


## Applying Jasper to a .NET Generic Host

We already saw above how to call `UseJasper()` with no arguments to add Jasper with all the defaults, but outside of using Jasper as just an in memory mediator, you'll need some further configuration.

If your Jasper configuration is relatively simple, you can modify the `JasperOptions` directly as shown in this overload of `UseJasper(Action<JasperOptions>)`:

snippet: sample_UseJasperWithInlineOptionsConfiguration

If you need to lookup configuration items like connection strings, ports, file paths, and other similar
items from application configuration -- or need to vary the Jasper configuration by hosting environment -- you
can use this overload:

snippet: sample_UseJasperWithInlineOptionsConfigurationAndHosting

Lastly, if you have more complex Jasper configuration, you may want to opt for a custom `JasperOptions`
type. Let's say we have a class called `CustomJasperOptions` that inherits from `JasperOptions`like this:

snippet: sample_CustomJasperOptions

That can be applied to a .NET Core application like this:

snippet: sample_UseJasperWithCustomJasperOptions





## Jasper with ASP.NET Core

Adding Jasper to an ASP.NET Core application -- with or without MVC -- isn't really any different. You still use the `UseJasper()` extension method like in this example:

snippet: sample_InMemoryMediatorProgram


## JasperOptions

The custom `JasperOptions` class shown below demonstrates the main features you can configure or extend for a Jasper application:


snippet: sample_JasperOptionsWithEverything

The major areas are:

* `ServiceName` -- see the section below
* `Extensions` -- see <[linkto:documentation/extensions]>
* `Advanced` -- most of these properties are related to message persistence. See <[linkto:documentation/durability]> for more information
* `Services` -- see <[linkto:documentation/ioc]>
* `Handlers` -- see <[linkto:documentation/execution]>
* `Endpoints` -- see <[linkto:documentation/integration/transports]>



