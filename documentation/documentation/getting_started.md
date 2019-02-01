<!--title: Getting Started-->

<[info]>
Jasper only targets Netstandard 2.0 at this time.
<[/info]>

Jasper is a framework for building services on .Net Core. The killer feature of Jasper (we think) is its very efficient command execution
pipeline that can be used as:

1. An alternative for building HTTP services with ASP.Net Core
1. A "mediator" type pipeline or an in memory messaging bus within a different framework like ASP.Net Core
1. When used in conjunction with low level messaging infrastructure tools like [RabbitMQ](https://www.rabbitmq.com/), a full fledged asynchronous messaging platform for robust communication and interaction between services
1. A lightweight service bus using its own transport mechanism
1. Any combination of the above

Jasper tries very hard to be a good citizen within the greater ASP.Net Core ecosystem and even when used in "headless" services, uses many elements of ASP.Net Core (logging, configuration, bootstrapping, hosted services) rather than try to reinvent something new. 

## Standalone Jasper Application

To create a standalone, headless Jasper service with no exposed HTTP endpoints, the quickest thing to do is to use a [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new?tabs=netcore21) template. First, install the latest `JasperTemplates` with this command:

```
dotnet new --install JasperTemplates
```

Next, build a new application using the `jasper` template in this case called "JasperApp" with this command:

```
dotnet new jasper -o JasperApp
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
Hosted Service: Jasper.Messaging.MessagingActivator
Hosted Service: Jasper.Messaging.Logging.MetricsCollector
Hosted Service: Jasper.Messaging.BackPressureAgent
Listening for loopback messages

Active sending agent to loopback://retries/

Application started. Press Ctrl+C to shut down.
```

Your new Jasper service isn't actually *doing* anything useful, but you're got a working skeleton. To learn more about what you can do with Jasper, see the <[linkto:documentation/tutorials]> page. See <[linkto:documentation/bootstrapping]> for more information about idiomatic Jasper bootstrapping.

That covers bootstrapping Jasper by itself, but next let's see how you can add Jasper
to an idiomatic ASP.Net Core application.



## Adding Jasper to an ASP.Net Core Application

While you may certainly build headless services with Jasper, it's pretty likely that you will also want to integrate Jasper into
ASP.Net Core applications.

If you prefer to use typical ASP.Net Core bootstrapping or want to add Jasper messaging support to an existing project, you can use the `UseJasper<T>()` extension method on ASP.Net Core's `IWebHostBuilder` as shown below:

<[sample:QuickStart-Add-To-AspNetCore]>

See <[linkto:documentation/bootstrapping/aspnetcore]> for more information about configuring Jasper through ASP.Net Core hosting.


## Your First HTTP Endpoint

The obligatory "Hello World" http endpoint is just this:

<[sample:QuickStartHomeEndpoint]>

As long as that class is in the main application assembly, Jasper will find it and make the "Get" method handle the root url of your application.

See <[linkto:documentation/http]> for more information about Jasper's HTTP handling features.


## Your First Message Handler

Let's say you're building an invoicing application and your application should handle an
`InvoiceCreated` event. The skeleton for the message handler for that event would look like this:

<[sample:QuickStart-InvoiceCreated]>

See <[linkto:documentation/messaging/handling/handlers]> for more information on message handler actions.
