<!--title: Logging Integration -->

The Jasper messaging is instrumented with the 
[ASP.Net Core ILogger abstraction and infrastructure](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/logging/?tabs=aspnetcore2x) throughout now. The logging would be
configured just like an ASP.Net Core application using `JasperRegistry.Hosting.ConfigureLogging()` or the normal `IWebHostBuilder.ConfigureLogging()` method
you may already be using.

There are two subjects:

1. *Jasper.Transports* - information about Jasper's running <[linkto:transports;title=transports]>, including circuit breaker events and messaging failures
1. *Jasper.Messages* - information about specific messages






