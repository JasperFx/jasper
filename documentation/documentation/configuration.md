<!--title: Configuration-->

The simplest way to set up configuration is to use the default behavior:

1. Add a json file to the project called appsettings.config
2. Add a class ending with `Settings` that has properties that map to appsettings.config
3. Include your `Settings` class in the constructor of a class and Jasper will automatically inject the settings object

<[sample:inject-settings]>

## ConfigurationBuilder

Jasper uses the [.NET Core ConfigurationBuilder](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration) to get config data and bind it to objects. The ConfigurationBuilder is exposed using the `Build` method. Use this method to add additional configuration sources. Calling this method will override the default build configuration, which is just "appsettings.config".

<[sample:build-configuration]>

If a settings class needs additional information to bind correctly, such as being in a nested sub-section, use the `Configure` method.

<[sample:configure-settings]>

## Modify Settings

It may be necessary to modify a settings object after it has been loaded from configuration.  Settings can be altered:

<[sample:alter-settings]>

or completely replaced:

<[sample:replace-settings]>

## Modify Application

The `JasperRegistry` (or the application that inherits from `JasperRegistry`) can be modified using loaded settings:

<[sample:with-settings]>
