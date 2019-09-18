<!--title:Jasper Command Line Support-->

<[info]>
Jasper uses the related [Oakton and Oakton.AspNetCore](https://jasperfx.github.io/oakton) projects for command line parsing and its command runner extensibility. "Oakton" is
a small community just to the North of Jasper's namesake.
<[/info]>

At this time, the Jasper team is focused on hosting applications either in IIS (or nginx) or as a console application that would be suitable for
running in a Docker container. To that end, we've added the `JasperHost` static class as a helper for standing up Jasper in a console application. You obviously want to run the application from a command line, and Jasper certainly does that, but the real value is the additional diagnostic commands
documented on this page that will help you diagnose problems or just generally understand your Jasper application better. The command line usage is also extensible.

If you're using `IWebHostBuilder` to bootstrap your application, you can opt into Jasper's expanded command line support with code similar to this hybrid MVC Core / Jasper application that utilizes an extension method called `RunJasper(args)` to execute a Jasper application at the command line:

<[sample:MvcCoreHybrid.Program]>

Likewise, this sample usage from the <[linkto:documentation/getting_started;title=getting started]> topic for a headless Jasper application
 could look like this:

<[sample:QuickStartConsoleMain]>

At runtime, `JasperHost` can use the `JasperRegistry` you hand it to <[linkto:documentation/bootstrapping;title=bootstrap a IJasperHost]> and run the application until the console process is stopped.

Or again, if you'd prefer to bootstrap with `IWebHostBuilder`, you can still use `JasperHost` like this:

<[sample:simplest-aspnetcore-run-from-command-line]>

You can also use the command line arguments to customize how the application runs like this:

<[sample:JasperHost-programmatic-customization]>

Or like this:

<[sample:JasperAgent-programmatic-construction]>

Let's say that your Jasper application compiles to `MyApp.exe` and uses the `JasperHost` class
to run the commands. In that case you can run your application simply by typing `MyApp` at the
command line with no arguments.



## Custom Commands

The Jasper uses the [Oakton.AspNetCore](https://jasperfx.github.io/oakton/documentation/aspnetcore/) library for its command line support. You can add custom commands to your Jasper application by simply including `OaktonCommand<T>` classes in either the main application assembly or in any assembly that is decorated with the `[JasperModule]` attribute like so:

<[sample:UseJasperModule-with-Extension]>

or without any kind of extension like so:

<[sample:AppWithExtensions]>


## Message Storage

See <[linkto:documentation/messaging/transports/durable]> for documentation on using the command line tooling to manage message storage.

