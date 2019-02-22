<!--title:Scheduled Message Delivery and Execution-->

<[info]>
Whenever possible, Jasper tries to use any kind of scheduled delivery functionality native to the underlying transport. If there is no native facility,
this is done by polling against the configured message storage of your Jasper system.
<[/info]>

You can send messages with Jasper, but request that the processing of the message happen at some later time with `IMessageContext.ScheduleSend()`:

<[sample:send-delayed-message]>

The message itself is sent out immediately, but when the receiving application sees that the message is scheduled to be processed later, the received message will be moved into the "delayed job processor" in the receiving application.

This functionality is useful for long lived workflows where there are time limits for any part of the process.

As of today, the only supported option is the default in memory option that we think is sufficient for short lived retries and limited workflow validations. In the longer term, Jasper will support database persisted delayed jobs. Follow [this GitHub issue](https://github.com/JasperFx/jasper/issues/199) for any progress on that front.

## Scheduling Jobs Locally

You also have the ability to schedule a message to be processed locally in the current system at a later time:

<[sample:schedule-job-locally]>
