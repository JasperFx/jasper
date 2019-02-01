<!--title:Long Running Processes with IHostedService-->

Jasper supports the ASP.Net [IHostedService](https://www.stevejgordon.co.uk/asp-net-core-2-ihostedservice)
interface to start and stop long running processes or startup and teardown actions in addition to the messaging or
http handler support.

Jasper itself uses this mechanism to run background processing for the durable messaging, starting up the actual message transports,
and metrics aggregation with some add ons.

To build a custom, long running process, see this example that just periodically sends a "ping" message:

<[sample:PingSender]>

To add `PingSender` to your application, it's just this code:

<[sample:AppThatUsesPingHandler]>

Now, when you run the application holding the code above, the `PingSender.StartAsync()` method will be called
as part of bootstrapping. The corresponding `PingSender.StopAsync()` will be called at application shutdown.