<!--title: Getting Started-->

<[info]>
Jasper only targets Netstandard 2.0 at this time.
<[/info]>

Jasper is a framework for command processing inside of .Net Core services. The command execution pipeline can be used as:

1. A "mediator" type pipeline or an in memory messaging bus within a different framework like ASP.Net Core
1. Used as a service bus in conjunction with topic-based queues like [RabbitMQ](https://www.rabbitmq.com/) for asynchronous messaging between services
1. A lightweight service bus using its own transport mechanism
1. An alternative for building HTTP services with ASP.Net Core
1. Any combination of the above

Jasper can either be in charge of your service's lifecycle as the primary application framework, or be added to an existing ASP.Net Core application. 
Jasper tries very hard to be a good citizen within the greater ASP.Net Core ecosystem. 

## Standalone Jasper Application

<[info]>
Even internally, Jasper uses ASP.Net Core's Hosting model for bootstrapping
<[/info]>

To create a standalone, headless Jasper service, the quickest thing to do is to use a [dotnet new](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-new?tabs=netcore21) template. First, install the latest `JasperTemplates` with this command:

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
