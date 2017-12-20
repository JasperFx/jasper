<!--title: Customizing Message Logging -->

<div class="alert alert-success"><b>Note!</b> The logging is combinatorial, meaning that you can use as many IMessageLogger or ITransportLogger strategies as you want and all of them will be called for each bus event.</div>

Jasper supports the idea of semantic or structured logging inside the messaging support through a pair of interfaces
that allow users to listen for messaging events:

* `IMessageLogger` - Listen to message level events like an envelope being queued for sending, process starting and finishing, and unroutable messages
* `ITransportLogger` - Listen to events within the various transports for events message batch failures, successes, or circuit breaks


You might add custom bus loggers to delegate to tools like [Serilog](https://serilog.net/) or try to hook into additional error handling.


## Console Logging

It's pretty verbose, but if you want to enable console logging at development time, use this flag inside of your `JasperRegistry`:

<[sample:UsingConsoleLoggingApp]>

If you're using the <[linkto:documentation/bootstrapping/console;title=Jasper.CommandLine]> add on, the `-v` or `--verbose` flags enable
the verbose console logging.

## Message Logging

You can create and register custom handlers to subscribe to all message level events with the `IMessageLogger` interface shown below:

<[sample:IMessageLogger]>

If you want to build a custom logger that only listens to *some* of the bus events, there's a `MessageLoggerBase` base class as a helper
for that pattern shown below:

<[sample:SampleMessageLogger]>


## Transport Logging

Similar to the message level logging, you can register custom strategies for the `ITransportLogger` interface shown below:

<[sample:ITransportLogger]>

Similar to the message logging shown above, you can use the `TransportLoggerBase` base class as a helper for implementing a custom
transport logger with support for only the events you care about as shown below:

<[sample:SampleTransportLogger]>

## Registering Custom Loggers


To register custom loggers, use this syntax in your `JasperRegistry`:

<[sample:AppWithCustomLogging]>






