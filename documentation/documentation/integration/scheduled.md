<!--title:Scheduled Message Delivery and Execution-->

<[info]>
 If there is no native facility,
this is done by polling against the configured message storage of your Jasper system. If there is no message persistence, the message scheduling is done in memory and scheduled messages will be lost if the application is stopped.
<[/info]>

You can send messages with Jasper, but request that the actual message sending to happen at some later time with `IMessageContext.ScheduleSend()`:

<[sample:send-delayed-message]>


This functionality is useful for long lived workflows where there are time limits for any part of the process. Internally, this feature is used for the *retry later* function in <[linkto:documentation/execution/errorhandling;error handling and retries]>. 

This same functionality is used for the `ICommandBus.Schedule()` functionality as shown below:

<[sample:schedule-job-locally]>

In the case above though, the message is executed locally at the designated time.

Here's a couple things to know about this functionality:

* Whenever possible, Jasper tries to use any kind of scheduled delivery functionality native to the underlying transport. The <[linkto:documentation/integration/transports/azureservicebus;title=Azure Service Bus transport]> uses native scheduled delivery for example
* If the Jasper application has some kind of configured <[linkto:documentation/durability;message persistence]>, the scheduled messages are persisted and durable even if the application is shut down and restarted. 
* The durable message scheduling can be used with multiple running nodes of the same application
* If there is no message persistence, the scheduling uses an in memory model. This model was really just meant for message retries in lightweight scenarios and probably shouldn't be used in high volume systems



