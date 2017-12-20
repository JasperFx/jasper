<!--title: Customizing Bus Logging -->

## Message Logging

<div class="alert alert-success"><b>Note!</b> The logging is combinatorial, meaning that you can use as many IMessageLogger's
as you want and all of them will be called for each bus event.</div>

You can register custom handlers to subscribe to all service bus events with the `IMessageLogger` interface shown below:

<[sample:IMessageLogger]>

You might add custom bus loggers to delegate to tools like [Serilog](https://serilog.net/) or try to hook into additional error handling.
If you want to build a custom logger that only listens to *some* of the bus events, there's a `MessageLoggerBase` base class as a helper
for that pattern shown below:

<[sample:SampleMessageLogger]>

To register that custom logger, use this syntax in your `JasperRegistry`:

<[sample:AppWithCustomLogging]>

Finally, you can quickly opt into verbose, console logging of bus events with this shorthand syntas:

<[sample:UsingConsoleLoggingApp]>


