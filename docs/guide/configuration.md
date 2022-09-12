# Configuration

::: warning
Jasper requires the usage of the [Lamar](https://jasperfx.github.io/lamar) IoC container, and the call
to `UseJasper()` quietly replaces the built in .NET container with Lamar.

Lamar was originally written specifically to support Jasper's runtime model as well as to be a higher performance
replacement for the older StructureMap tool.
:::

Jasper is configured with the `IHostBuilder.UseJasper()` extension methods, with the actual configuration
living on a single `JasperOptions` object.

## With ASP.NET Core

Below is a sample of adding Jasper to an ASP.NET Core application that is bootstrapped with
`WebApplicationBuilder`:

snippet: sample_Quickstart_Program

## "Headless" Applications

:::tip
The `JasperOptions.Services` property can be used to add additional IoC service registrations with
either the standard .NET `IServiceCollection` model or the [Lamar ServiceRegistry](https://jasperfx.github.io/lamar/guide/ioc/registration/registry-dsl.html) syntax.
:::

For "headless" console applications with no user interface or HTTP service endpoints, the bootstrapping
can be done with just the `HostBuilder` mechanism as shown below:

snippet: sample_bootstrapping_headless_service
