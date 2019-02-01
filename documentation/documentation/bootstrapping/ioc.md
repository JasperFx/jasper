<!--title:IoC Container Integration-->

<[info]>
If you're curious, in the real world *Lamar* is a slightly bigger town just up highway 71 from Jasper's namesake.
<[/info]>

Jasper **only** supports the [Lamar](http://github.com/jasperfx/lamar) IoC container.

See [Introducing BlueMilk: StructureMap’s Replacement & Jasper’s Special Sauce](https://jeremydmiller.com/2018/01/16/introducing-bluemilk-structuremaps-replacement-jaspers-special-sauce/) for more information on exactly how the Jasper + Lamar combination works (under the original "BlueMilk" codename that most people hated;)).


To register services in a Jasper application, use the `JasperRegistry.Services` root like this:

<[sample:JasperAppWithServices]>

<[linkto:documentation/extensions;title=Extensions]> can also register services, but Jasper will enforce a service registration precedence like this:

1. Application registrations from your `JasperRegistry.Services`
1. Extension registrations
1. Baseline Jasper and ASP.Net Core service registrations

What this means is that registrations made in your application's `JasperRegistry` will always win out over extensions and the base framework.

## Accessing the Raw Container

The best practice in theory states that you should never need to access the underlying IoC container in your application after the initial bootstrapping, but there's always some reason (testing?), so you can do this:

<[sample:GetAtTheContainer]>


