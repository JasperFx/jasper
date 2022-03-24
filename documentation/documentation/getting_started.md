<!--title: Getting Started-->

::: tip warning
Jasper only targets Netstandard 2.1 at this time and supports .Net Core 3.* runtimes.
:::

Jasper is a toolset for command execution and message handling within .Net Core applications. The killer feature of Jasper (*we* think) is its very efficient command execution
pipeline that can be used as:

1. A "mediator" type pipeline or an in memory messaging bus and command executor within .Net Core 3.* applications
1. When used in conjunction with low level messaging infrastructure tools like [RabbitMQ](https://www.rabbitmq.com/), a full fledged asynchronous messaging platform for robust communication and interaction between services

Jasper tries very hard to be a good citizen within the greater ASP.Net Core ecosystem and even when used in "headless" services, uses many elements of ASP.Net Core (logging, configuration, bootstrapping, hosted services) rather than try to reinvent something new. As of v1.0, Jasper utilizes the new [.Net Generic Host](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host?view=aspnetcore-3.1) for bootstrapping and application teardown. This makes Jasper relatively easy to use in combination with many of the most popular .Net Core tools.

## Standalone Jasper Application

To create a standalone, headless Jasper service with no exposed HTTP endpoints, the quickest thing to do is to use a [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new?tabs=netcore21) template. First, install the latest `JasperTemplates` Nuget with this command:

```
dotnet new --install JasperTemplates
```

Next, build a new application using the `jasper.service` template in this case called "JasperApp" with this command:

```
dotnet new jasper.service -o JasperApp
```

Finally, if you run this new application with this command:

```
cd JasperApp
dotnet run
```

You should see some output in the console describing the running Jasper application like this:

```
Running service 'JasperConfig'
Application Assembly: JasperApp, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Hosting environment: Production
Content root path: /SomeDirectory/JasperApp/bin/Debug/netcoreapp2.1/

Application started. Press Ctrl+C to shut down.
```

Your new Jasper service isn't actually *doing* anything useful, but you're got a working skeleton. To learn more about what you can do with Jasper, see the <[linkto:documentation/samples]> page. See <[linkto:documentation/bootstrapping]> for more information about idiomatic Jasper bootstrapping.

That covers bootstrapping Jasper by itself, but next let's see how you can add Jasper
to an idiomatic ASP.Net Core application.



## Adding Jasper to an ASP.Net Core Application

While you may certainly build headless services with Jasper, it's pretty likely that you will also want to integrate Jasper into
ASP.Net Core applications.

If you prefer to use typical ASP.Net Core bootstrapping or want to add Jasper messaging support to an existing project, you can use the `UseJasper<T>()` extension method on ASP.Net Core's `IHostBuilder` as shown below:

snippet: sample_QuickStart_Add_To_AspNetCore

See <[linkto:documentation/bootstrapping]> for more information about configuring Jasper through the unified .Net Core `IHostBuilder`.


## Your First Message Handler

Let's say you're building an invoicing application and your application should handle an
`InvoiceCreated` event. The skeleton for the message handler for that event would look like this:

snippet: sample_QuickStart_InvoiceCreated

As long as this **public** class is inside of your main application assembly, Jasper is going to find this automatically and write this handler up to its execution pipeline. 

And don't worry, you're more than able to author asynchronous message handlers, and probably should in any case involving calls to external systems, but Jasper
is able to adapt to **your** code rather than making you shoehorn your code into a framework adapter interface.

See <[linkto:documentation/execution/handlers]> for more information on message handler actions.
