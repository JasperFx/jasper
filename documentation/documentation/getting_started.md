<!--title: Getting Started-->

<[info]>
Jasper only targets Netstandard 2.0 and higher at this time.
<[/info]>

Jasper is a framework for building server side services in .Net. Jasper can be used as an alternative web framework for .Net, a service bus for messaging, as a "mediator" type
pipeline within a different framework, or any combination thereof. Jasper can be used as either your main application framework that handles all the configuration and bootstrapping, or as an add on to ASP.Net Core applications.



## Standalone Jasper Application

```
dotnet new --install JasperTemplates


Jeremys-MacBook-Pro:jasper jeremydmiller$ cd ..
Jeremys-MacBook-Pro:code jeremydmiller$ mkdir JasperApp
Jeremys-MacBook-Pro:code jeremydmiller$ cd JasperApp
Jeremys-MacBook-Pro:JasperApp jeremydmiller$ dotnet new jasper
The template "Jasper Service" was created successfully.
Jeremys-MacBook-Pro:JasperApp jeremydmiller$ ls
JasperApp.csproj	JasperConfig.cs		Program.cs		appsettings.json
Jeremys-MacBook-Pro:JasperApp jeremydmiller$ dotnet run



```




## Adding Jasper to ASP.Net Core Application



To create a new Jasper application, start by building a new console application:

<pre>dotnet new console -n MyApp</pre>

Then, go get Jasper from Nuget:

PM> Install-Package Jasper
Or, using paket:

paket add nuget Jasper

While this isn't expressly necessary, you probably want to create a new `JasperRegistry` that will define the active options and configuration for your application:

<[sample:MyAppRegistry]>

See <[linkto:documentation/bootstrapping/configuring_jasper]> for more information about using the `JasperRegistry` class.

Now, to bootstrap your application, add this code to the entrypoint of your console application:

<[sample:QuickStartConsoleMain]>

By itself, this doesn't really do much, so let's start listening for messages from other applications using Jasper's built in, lightweight transport.


<[sample:MyAppRegistryWithOptions]>

Now, when you run the console application you should see output like this:

```
Hosting environment: Production
Content root path: /Users/jeremill/code/jasper/src/MyApp/bin/Debug/netcoreapp2.0
Listening for messages at loopback://delayed/
Listening for messages at jasper://localhost:2333/replies
Listening for messages at jasper://localhost:2222/incoming
Application started. Press Ctrl+C to shut down.
```

See <[linkto:documentation/bootstrapping]> for more information about idiomatic Jasper bootstrapping.

That covers bootstrapping Jasper by itself, but next let's see how you can add Jasper
to an idiomatic ASP.Net Core application.

## Adding Jasper to an ASP.Net Core Application


<[info]>
As of Jasper 0.7, the ASP.Net Core integration was folded right back into the main library with optimized bootstrapping and there is no more "Jasper.Http" library.
<[/info]>

While you may certainly build headless services with Jasper, it's pretty likely that you will also want to integrate Jasper into
ASP.Net Core applications.

If you prefer to use typical ASP.Net Core bootstrapping or want to add Jasper messaging support to an existing project, you can use the `UseJasper<T>()` extension method on ASP.Net Core's `IWebHostBuilder` as shown below:

<[sample:QuickStart-Add-To-AspNetCore]>

The `T` constraint will refer to the Jasper configuration for your hybrid Jasper/ASP.Net Core application, this time in a subclass of the `JasperHttpRegistry` class that's just a specialized version of
`JasperRegistry`:

<[sample:ServiceBusApp]>

See <[linkto:documentation/bootstrapping/aspnetcore]> for more information about configuring Jasper through ASP.Net Core hosting.


## Your First HTTP Endpoint

The obligatory "Hello World" http endpoint is just this:

<[sample:QuickStartHomeEndpoint]>

As long as that class is in the same assembly as your `JasperHttpRegistry` class, Jasper will find it and make the "Get" method handle the root url of your application.

See <[linkto:documentation/http]> for more information about Jasper's HTTP handling features.


## Your First Message Handler

Let's say you're building an invoicing application and your application should handle an
`InvoiceCreated` event. The skeleton for the message handler for that event would look like this:

<[sample:QuickStart-InvoiceCreated]>

See <[linkto:documentation/messaging/handling/handlers]> for more information on message handler actions.
