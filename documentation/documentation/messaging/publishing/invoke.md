<!--title:Invoking or Enqueuing a Message Locally-->

Assuming that your application has a message handler for a given message like `InvoiceCreated`
in the sample below, you can consume and process that message inline with this code:

<[sample:IServiceBus.Invoke]>

The `Invoke()` method will **not** apply any error policies to retry the message if it fails and you will see exceptions bubble up on failures. This mechanism will process <[linkto:documentation/messaging/handling/cascading]> if the message succeeds.


As an alternative, you can enqueue a message to a local queue to be handled later by the currently running node with `IServiceBus.Enqueue()`:

<[sample:IServiceBus.Enqueue]>

The `InvoiceCreated` message above will be enqueued locally in the application's default channel. By default, that's one of the <[linkto:documentation/messaging/transports/loopback]> channels, but you can override that to any channel Uri that supports local enqueuing like so:

<[sample:SetDefaultChannel]>

Today that pretty well means using the loopback transport, but before too long the <[linkto:documentation/messaging/transports/durable]> will be usable locally as well for persistent queueing in this case. See [this GitHub issue](https://github.com/JasperFx/jasper/issues/179) to track the progress.


