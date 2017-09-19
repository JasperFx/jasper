<!--title:Delayed Job Processing-->

You can send messages with Jasper, but request that the processing of the message happen at some later time with `IServiceBus.DelaySend()`:

<[sample:send-delayed-message]>

The message itself is sent out immediately, but when the receiving application sees that the message is scheduled to be processed later, the received message will be moved into the "delayed job processor" in the receiving application.

This functionality is useful for long lived workflows where there are time limits for any part of the process.

As of today, the only supported option is the default in memory option that we think is sufficient for short lived retries and limited workflow validations. In the longer term, Jasper will support database persisted delayed jobs. Follow [this GitHub issue](https://github.com/JasperFx/jasper/issues/199) for any progress on that front.

