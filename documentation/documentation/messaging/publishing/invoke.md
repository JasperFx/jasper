<!--title:Invoking or Enqueuing a Message Locally-->

Assuming that your application has a message handler for a given message like `InvoiceCreated`
in the sample below, you can consume and process that message inline with this code:

<[sample:IServiceBus.Invoke]>

The `Invoke()` method will **not** apply any error policies to retry the message if it fails and you will see exceptions bubble up on failures. This mechanism will process <[linkto:documentation/messaging/handling/cascading]> if the message succeeds.


As an alternative, you can enqueue a message to a local queue to be handled later by the currently running node with `IMessageContext.Enqueue()`:

<[sample:IServiceBus.Enqueue]>

The `InvoiceCreated` message above will be enqueued locally using the application's rules for worker queue routing and durability. See <[linkto:documentation/messaging/handling/workerqueues]> for information about how to control these factors to give certain message types more or less priority and to tell Jasper which message types should be durable.


Today that pretty well means using the loopback transport, but before too long the <[linkto:documentation/messaging/transports/durable]> will be usable locally as well for persistent queueing in this case. See [this GitHub issue](https://github.com/JasperFx/jasper/issues/179) to track the progress.


## Enqueue to a Specific Worker Queue

If you want to enqueue a message locally to a specific worker queue, you can use this syntax:

<[sample:IServiceBus.Enqueue-to-specific-worker-queue]>

See <[linkto:documentation/messaging/handling/workerqueues]> for more information about worker queues.