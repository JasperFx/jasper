<!--title: Customizing Bus Logging -->

<div class="alert alert-success"><b>Note!</b> The logging is combinatorial, meaning that you can use as many IBusLogger's
as you want and all of them will be called for each bus event.</div>

You can register custom handlers to subscribe to all service bus events with the `IBusLogger` interface shown below:

<[sample:IBusLogger]>

You might add custom bus loggers to delegate to tools like [Serilog](https://serilog.net/) or try to hook into additional error handling.
If you want to build a custom logger that only listens to *some* of the bus events, there's a `BusLoggerBase` base class as a helper
for that pattern shown below:

<[sample:SampleBusLogger]>

To register that custom logger, use this syntax in your `JasperRegistry`:

<[sample:AppWithCustomLogging]>

Finally, you can quickly opt into verbose, console logging of bus events with this shorthand syntas:

<[sample:UsingConsoleLoggingApp]>


