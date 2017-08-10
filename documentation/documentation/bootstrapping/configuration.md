<!--title: Application Configuration and Settings-->

<div class="alert alert-info"><b>Note!</b> All of the code snippets shown in this topic apply to the JasperRegistry syntax</div>

For application configuration, Jasper supports both the built in .Net Core configuration and a form of strong typed configuration
we call the ["Settings" model that was originally used in FubuMVC](https://jeremydmiller.com/2014/11/07/strong_typed_configuration/). 

## Quick Start

Probably the most common scenario is to have a single configuration file mapped to a single object:

1. Add a class that ends with `Settings` to your project, e.g. `MySettings.cs`.
2. Add a json file that has properties that match your `Settings` class.
3. Use the `Build` method to tell Jasper about your configuration file.
4. Include your `Settings` class in the constructor of a class and Jasper will automatically inject the settings object

<[sample:inject-settings]>

## Configuration Lifecycle

Application configuration can come from a mix of the built in .Net Core configuration sources and programmatic options set in either your
`JasperRegistry` or a loaded extension. While you make all the declarations in your `JasperRegistry` class, Jasper takes some steps to execute the usage of configuration options at bootstrapping time like so:

1. Build out the .Net Core `IConfigurationRoot` object based on the sources added to `JasperRegistry.Configuration`.
1. Loads the default data for known `Settings` types
1. Apply all the `JasperRegistry.Settings.Alter()` or `Replace()` delegates from registered extensions in the order that they were registered
1. Apply all the `JasperRegistry.Settings.Alter()` or `Replace()` delegates configured in your `JasperRegistry` in the order that they were
   registered to ensure that the application specific options always win out over the base options or options coming from an extension
1. Finally, apply all the `JasperRegistry.Settings.With()` delegates configured in your `JasperRegistry` to use the final, configured versions of  
   `Settings` objects to alter your application setup

## Add Configuration Sources

Jasper uses the [.NET Core ConfigurationBuilder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) to get config data and bind it to objects.

<[sample:build-configuration]>

If you need to bind a settings class that does not follow the convention of ending with `Settings` then use the `Configure` method to tell Jasper which class you want to bind.

<[sample:configure-settings]>

If a settings class needs additional information to bind correctly, such as being in a nested sub-section, use the `Configure` method.

<[sample:configure-settings2]>

## Modify Settings

It may be necessary to modify a settings object after it has been loaded from configuration.  Settings can be altered:

<[sample:alter-settings]>

or completely replaced:

<[sample:replace-settings]>

## Modify Application

The `JasperRegistry` (or the application that inherits from `JasperRegistry`) can be modified using loaded settings:

<[sample:with-settings]>
