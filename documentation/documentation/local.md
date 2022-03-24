<!--title:Local Worker Queues-->


Jasper's <[linkto:documentation/execution]> can be consumed from in memory queues within your application. The queueing is all based around [ActionBlock](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/how-to-perform-action-when-a-dataflow-block-receives-data) objects from the [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) library. As such, you have a fair amount of control over parallelization and even some back pressure. These local queues can be used directly, or as a <[linkto:documentation/integration/transports;title=transport]> that uses the application's <[linkto:documentation/integration/routing;title=message routing rules]>.




## Enqueueing Messages Locally

::: tip warning
The `IMessagePublisher` and `IMessageContext` interfaces both implement the `ICommandBus` interface, and truth be told,
it's just one underlying concrete class and the interfaces just expose narrower or broader options.
:::

Using the `ICommandBus.Enqueue()` method, you can queue up messages to be executed asynchronously:

snippet: sample_enqueue_locally

This feature is useful for asynchronous processing in web applications or really any kind of application where you need some parallelization or concurrency. 

Some things to know about the local queues:

* Local worker queues can be durable, meaning that the enqueued messages are persisted first so that they aren't lost if the application is shut down before they're processed. More on that below.
* You can use any number of named local queues, and they don't even have to be declared upfront (might want to be careful with that though)
* Local worker queues utilize Jasper's <[linkto:documentation/execution/errorhandling]> policies to selectively handle any detected exceptions from the <[linkto:documentation/execution/handlers;title=message handlers]>
* You can control the priority and parallelization of each individual local queue
* Message types can be routed to particular queues
* <[linkto:documentation/execution/cascading;title=Cascading messages]> can be used with the local queues
* The local queues can be used like any other message transport and be the target of routing rules




## Default Queues

Out of the box, each Jasper application has a default queue named "default". In the absence of any
other routing rules, all messages enqueued to `ICommandBus` will be published to this queue.

## Local Message Routing

In the absence of any kind of routing rules, any message enqueued with `ICommandBus.Enqueue()` will just be handled by the 
*default* local queue. To override that choice on a message type by message type basis, you can use the `[LocalQueue]` attribute
on a message type:

snippet: sample_local_queue_routed_message

Otherwise, you can take advantage of the routing rules on the `JasperOptions.Endpoints.Publish()` method like this:

snippet: sample_LocalTransportApp

The routing rules and/or `[LocalQueue]` routing is also honored for cascading messages, meaning that any message that is handled inside a Jasper system could publish cascading messages to the local worker queues.


## Durable Local Messages

The local worker queues can optionally be designated as "durable," meaning that local messages would be persisted until they can be successfully processed to provide a guarantee that the message will be successfully processed in the case of the running application faulting or having been shut down prematurely (assuming that other nodes are running or it's restarted later of course).

Here is an example of configuring a local queue to be durable:

snippet: sample_LocalDurableTransportApp


See <[linkto:documentation/durability]> for more information.


## Scheduling Local Execution

The "scheduled execution" feature can be used with local execution within the same application. See <[linkto:documentation/integration/scheduled]> for more information. Use the `ICommandBus.Schedule()` methods like this:

snippet: sample_schedule_job_locally


## Configuring Parallelization and Execution Properties

The queues are built on top of the TPL Dataflow library, so it's pretty easy to configure parallelization (how many concurrent messages could be handled by a queue). Here's an example of how to establish this:

snippet: sample_LocalQueuesApp


## Explicitly Enqueue to a Specific Local Queue

If you want to enqueue a message locally to a specific worker queue, you can use this syntax:

snippet: sample_IServiceBus.Enqueue_to_specific_worker_queue


## Local Queues as a Messaging Transport


::: tip warning
The local transport is used underneath the covers by Jasper for retrying
locally enqueued messages or scheduled messages that may have initially failed.
:::

In the sample Jasper configuration shown below:

snippet: sample_LocalTransportApp

Calling `IMessagePublisher.Send(new Message2())` would publish the message to the local "important" queue. 
