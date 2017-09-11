<!--Title:Local Loopback Transport-->

<div class="alert alert-info"><b>Note!</b> The loopback transport is used underneath the covers by Jasper for retrying
locally enqueued messages or scheduled messages that may have initially failed.</div>


The "loopback" transport is a local, in memory transport that allows you to queue messages. This transport is enabled
by default, and the `Uri` structure is *loopback://queuename*. Here are some examples of how it is configured:

Do note that messages to any known queue will be processed without any kind of explicit configuration to make the 
loopback queues be "incoming."

<[sample:LoopbackTransportApp]>