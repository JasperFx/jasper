<!--Title:Local Loopback Transport-->

<div class="alert alert-info"><b>Note!</b> The loopback transport is used underneath the covers by Jasper for retrying
locally enqueued messages or scheduled messages that may have initially failed.</div>


The "loopback" transport is a local, in memory transport that allows you to queue messages directly to the internal <[linkto:documentation/messaging/handling/workerqueues;title=worker queues]>. This transport is enabled
by default, and the `Uri` structure is *loopback://queuename*, where "queuename" is the name of a worker queue. Here are some examples of how it is configured:

<[sample:LoopbackTransportApp]>

While you can use <[linkto:documentation/messaging/routing/static_routing;title=explicit publishing rules]> by message type to route messages locally to the loopback transport, it's probably easier to just use the explicit methods on `IServiceBus` shown below:

<[sample:enqueue-locally]>

See <[linkto:documentation/messaging/handling/workerqueues]> and <[linkto:documentation/messaging/transports/durable]> for more information.

