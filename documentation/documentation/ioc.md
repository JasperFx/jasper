<!--title:IoC Container Integration-->

<[warning]>
This is still a little bit in flight as BlueMilk itself is somewhat likely to be changed and possibly renamed.
<[/warning]>

Jasper **only** supports the [BlueMilk](http://github.com/jasperfx/bluemilk) IoC container.

See [Introducing BlueMilk: StructureMap’s Replacement & Jasper’s Special Sauce](https://jeremydmiller.com/2018/01/16/introducing-bluemilk-structuremaps-replacement-jaspers-special-sauce/) for more information on exactly how the Jasper + BlueMilk combination works.


To register services in a Jasper application, use the `JasperRegistry.Services` root like this:

<[sample:JasperAppWithServices]>
