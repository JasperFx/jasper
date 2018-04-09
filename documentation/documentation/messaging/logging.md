<!--title: Customizing Message Logging -->


The logging got pretty scrambled for v0.7, but the big change is that Jasper uses the
[ASP.Net Core ILogger abstraction and infrastructure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?tabs=aspnetcore2x) throughout now. The logging would be
configured just like an ASP.Net Core application using `JasperRegistry.Hosting.ConfigureLogging()` or the normal `IWebHostBuilder.ConfigureLogging()` method
you may already be using.



More to come in this space soon-ish....




