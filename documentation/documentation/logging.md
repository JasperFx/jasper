<!--title: Logging Integration -->

The Jasper messaging is instrumented with the
[ASP.NET Core ILogger abstraction and infrastructure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?tabs=aspnetcore2x) throughout now. The logging would be
configured just like an ASP.NET Core application using `JasperRegistry.Hosting.ConfigureLogging()` or the normal `IWebHostBuilder.ConfigureLogging()` method
you may already be using.

There are two subjects:

1. *Jasper.Transports* - information about Jasper's running <[linkto:documentation/integration/transports;title=transports]>, including circuit breaker events and messaging failures
1. *Jasper.Messages* - information about specific messages






