<!--title:Scheduled Message Delivery and Execution-->


<[info]>
Why didn't we just use [Hangfire](https://www.hangfire.io/)? We looked at it, but thought that our current solution avoided having to have more dependencies and the database mechanism was actually easier for diagnostic views of the scheduled messages. We do recommend Hangfire if you really just want scheduled job execution.
<[/info]>


## Schedule Message Delivery

You can send messages with Jasper, but request that the actual message sending to happen at some later time with `IMessageContext.ScheduleSend()`:

<[sample:send-delayed-message]>


This functionality is useful for long lived workflows where there are time limits for any part of the process. Internally, this feature is used for the *retry later* function in <[linkto:documentation/execution/errorhandling;error handling and retries]>. 

Here's a couple things to know about this functionality:

* Whenever possible, Jasper tries to use any kind of scheduled delivery functionality native to the underlying transport. The <[linkto:documentation/integration/transports/azureservicebus;title=Azure Service Bus transport]> uses native scheduled delivery for example
* If the Jasper application has some kind of configured <[linkto:documentation/durability;message persistence]>, the scheduled messages are persisted and durable even if the application is shut down and restarted. 
* The durable message scheduling can be used with multiple running nodes of the same application
* If there is no message persistence, the scheduling uses an in memory model. This model was really just meant for message retries in lightweight scenarios and probably shouldn't be used in high volume systems



## Schedule Execution Locally

This same functionality is used for the `ICommandBus.Schedule()` functionality as shown below:

<[sample:schedule-job-locally]>

In the case above though, the message is executed locally at the designated time.

The locally scheduled messages are handled in the local "durable" queue. You can fine tune the parallelization of this <[linkto:documentation/local;title=local worker queue]> through this syntax:

<[sample:DurableScheduledMessagesLocalQueue]>


## Schedule Execution From Cascading Messages

To schedule a message to another system as a <[linkto:documentation/execution/cascading;title=cascading message]> from a message handler, 
you can return the `ScheduledResponse` object like this:

<[sample:DelayedResponseHandler]>


## RequeueLater

The `RetryLater()` mechanism in message error handling is using the scheduled execution. See <[linkto:documentation/execution/errorhandling]> for more information.

