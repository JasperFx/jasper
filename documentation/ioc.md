<!--title:IoC Container Integration-->

<[info]>
If you're curious, in the real world *Lamar* is a slightly bigger town just up US Highway 71 from Jasper's namesake.
<[/info]>

Jasper **only** supports the [Lamar](http://github.com/jasperfx/lamar) IoC container and `UseJasper()` will
also replace the built in ASP.Net Core DI container with Lamar.

Now, since we are using Lamar as the IoC container, 
let's look at the services that get added to the application container by Jasper. First, add a Nuget reference to [Lamar.Diagnostics](https://jasperfx.github.io/lamar/documentation/ioc/aspnetcore/).
Once that Nuget is applied, we have some extra command line options in our application we can use to understand our new
.Net Core application. Using this command from the root of the project: 

```
dotnet run -- lamar-services -a Jasper
```

Which I'm going to leave up to anyone curious enough to do because it's a bit of text that's likely to change. The big additions you care about are:

1. `MessagingRoot` as an `IHostedService`. This class holds all the background Jasper work for sending and receiving messages.
1. `ICommandBus` - a service that you can use to execute or enqueue commands or messages within your local application
1. `IMessagePublisher` - a service that you can use to send or publish outgoing messages in addition to everything that `ICommandBus` does
1. `IMessageContext` - a service that allows you to do everything that `IMessagePublisher` does, but also adds functionality to enroll in outbox messaging transactions

The IoC integration with command/message processing is a little different in Jasper than most
other similar tools in .Net. 


See [Introducing BlueMilk: StructureMap’s Replacement & Jasper’s Special Sauce](https://jeremydmiller.com/2018/01/16/introducing-bluemilk-structuremaps-replacement-jaspers-special-sauce/) for more information on exactly how the Jasper + Lamar combination works (under the original "BlueMilk" codename that most people hated;)).


To register services in a Jasper application, use the `JasperOptions.Services` root like this:

<[sample:JasperAppWithServices]>

<[linkto:extensions;title=Extensions]> can also register services, but Jasper will enforce a service registration precedence like this:

1. Application registrations from your `JasperOptions.Services`
1. Extension registrations
1. Baseline Jasper and ASP.Net Core service registrations

What this means is that registrations made in your application's `JasperOptions` will always win out over extensions and the base framework.


## Accessing the Raw Container

The best practice in theory states that you should never need to access the underlying IoC container in your application after the initial bootstrapping, but there's always some reason (testing?) to do so and there's a **lot** of functionality in [Lamar](https://jasperfx.github.io/lamar) that isn't exposed through the simple `IServiceProvider` abstraction, so you can do this:

<[sample:GetAtTheContainer]>

Or, you can always inject the current `IContainer` as a constructor argument into any service that is resolved from the container.


