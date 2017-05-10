<!--title: Customizing Bus Logging -->

It's all about the `IBusLogger` interface.

You can enable basic logging using `Logging.UseConsoleLogging = true`.  You can add a custom logger by implementing the `IBusLogger` interface and registering your logger with `Logging.LogBusEventsWith<MyBusLogger>()`.

## Transport Logger

If your chosen transport supports logging, you can add a custom `ITransportLogger`.

```csharp
Logging.LogTransportEventsWith<ConsoleTransportLogger>();
```
