<!--title:Jasper as a Local Command Bus-->

## Invoke a Message Directly

Assuming that your application has a message handler for a given message like `InvoiceCreated`
in the sample below, you can consume and process that message inline with this code:

<[sample:IServiceBus.Invoke]>

The `Invoke()` method will **not** apply any error policies to retry the message if it fails and you will see exceptions bubble up on failures. This mechanism will process <[linkto:documentation/handling/cascading]> if the message succeeds.


As an alternative, you can enqueue a message to a local queue to be handled later by the currently running node with `IMessageContext.Enqueue()`:

<[sample:IServiceBus.Enqueue]>

The `InvoiceCreated` message above will be enqueued locally using the application's rules for worker queue routing and durability. See <[linkto:documentation/handling/workerqueues]> for information about how to control these factors to give certain message types more or less priority and to tell Jasper which message types should be durable.


Today that pretty well means using the loopback transport, but before too long the <[linkto:documentation/durability]> will be usable locally as well for persistent queueing in this case. See [this GitHub issue](https://github.com/JasperFx/jasper/issues/179) to track the progress.

## Local Worker Queues

To provide more fine-grained control over how messages are handled within your application, you can assign messages to 
named "worker queues" within the application that have configurable priority through a maximum number of concurrent threads handling messages within that worker queue. 

Also, the worker queues can optionally be designated as "durable," meaning that local, loopback messages would be persisted until they can be successfully processed to provide a guarantee that the message will be successfully processed in the case of the running application faulting or having been shut down prematurely (assuming that other nodes are running or it's restarted later of course).

See <[linkto:documentation/messaging/transports/durable]> and <[linkto:documentation/messaging/transports/loopback]> for more information.

Worker queue assignment and configuration can be made through the `JasperRegistry` fluent interface like so:

<[sample:AppWithWorkerQueues]>


Alternatively, you can create a worker queue for a specific message type by using the `[Worker]` attribute on a message
type as shown below:

<[sample:using-WorkerAttribute]>

This attribute will configure the worker queue with the designated queue name, parallelization, and durability. Do note that in the event of a name conflict between the attributes and named worker queues in the `JasperRegistry` class for the system, the attributes are processed last and would win out. 



## Enqueue to a Specific Local Queue

If you want to enqueue a message locally to a specific worker queue, you can use this syntax:

<[sample:IServiceBus.Enqueue-to-specific-worker-queue]>

See <[linkto:documentation/local/workerqueues]> for more information about worker queues.


## Local Queues as a Messaging Transport


<[info]>
The local transport is used underneath the covers by Jasper for retrying
locally enqueued messages or scheduled messages that may have initially failed.
<[/info]>


The "loopback" transport is a local, in memory transport that allows you to queue messages directly to the internal <[linkto:documentation/messaging/handling/workerqueues;title=worker queues]>. This transport is enabled
by default, and the `Uri` structure is *loopback://queuename*, where "queuename" is the name of a worker queue. Here are some examples of how it is configured:

<[sample:LoopbackTransportApp]>

While you can use <[linkto:documentation/messaging/configuration;title=explicit publishing rules]> by message type to route messages locally to the loopback transport, it's probably easier to just use the explicit methods on `IMessageContext` shown below:

<[sample:enqueue-locally]>

See <[linkto:documentation/messaging/handling/workerqueues]> and <[linkto:documentation/messaging/transports/durable]> for more information.



