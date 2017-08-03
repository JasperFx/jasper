<!--title: Application Configuration-->

## Quick Start

Probably the most common scenario is to have a single configuration file mapped to a single object:

1. Add a class that ends with `Settings` to your project, e.g. `MySettings.cs`.
2. Add a json file that has properties that match your `Settings` class.
3. Use the `Build` method to tell Jasper about your configuration file.
4. Include your `Settings` class in the constructor of a class and Jasper will automatically inject the settings object

<[sample:inject-settings]>

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
