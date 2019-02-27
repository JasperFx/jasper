## Jasper Http service

There's just a couple pieces here:

1. The project file makes a reference to *Jasper* 
1. Any Jasper listeners and publishers can be configured in the `JasperConfig` class
1. All other ASP.Net Core configuration is in `Startup`
1. The `Program.Main()` method bootstraps and runs the Jasper application

For more information, see the [Jasper Documentation](http://jasperfx.github.io/documentation/)
