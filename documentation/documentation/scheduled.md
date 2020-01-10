<!--title:Scheduling Message Execution-->

<[info]>
This functionality is perfect for "timeout" conditions like "send an email if this issue isn't solved within 3 days." Internally,
Jasper uses this functionality for the "retry later" error handling.
<[/info]>

Most times you probably want a message whether <[linkto:documentation/publishing/invoke;title=enqueued locally]> or 
<[linkto:documentation/publishing/pubsub;title=published to another system]>, to be processed as soon as 
possible. In some cases though, you may want to say that the work represented by handling a message should be processed later, either by a specified time delay or at a certain time in the future. For this reason, Jasper supports the *scheduled message execution* to do just that. Unlike many other tools, Jasper depends on the downstream receivers being responsible for scheduling the execution rather than trying to do a delayed delivery of the messages. For now, this pretty well means that there needs to be another Jasper application on the other
side.

## How it Works

<[info]>
Why didn't we just use [Hangfire](https://www.hangfire.io/)? We looked at it, but thought that our current solution avoided having to have more dependencies and the database mechanism was actually easier for diagnostic views of the scheduled messages. We do recommend Hangfire if you really just want scheduled job execution.
<[/info]>

No points for style here, the scheduled message execution simply polls in a background agent for any messages that are ready to execute. There is an in memory model by default that's meant to be just good enough for the message retries, but for any kind of durable scheduling that can survive beyond one node process, please use either the <[linkto:documentation/extensions/marten/persistence;title=Marten-backed message persistence]> or the 
<[linkto:documentation/extensions/sqlserver/persistence;title=Sql Server-backed persistence]>.

In both cases, there is a guarantee that each scheduled message will be executed on only one node. 


## Schedule Send Message to Another System

Likewise, you can send a message to another system and request that the message be executed later, either by a time delay:

<[sample:ScheduleSend-In-3-Days]>

or at a certain time:

<[sample:ScheduleSend-At-5-PM-Tomorrow]>

If you wanted to, the methods above are really just syntactical sugar for this below:

<[sample:ScheduleSend-Yourself]>

## Schedule Execution Locally

You can schedule a message to be executed in the local system at a scheduled time. Either by specifying a `TimeSpan` delay like this:

<[sample:ScheduleLocally-In-3-Days]>

Or specify the execution at an exact time like this:

<[sample:ScheduleLocally-At-5-PM-Tomorrow]>

Note that if you are using <[linkto:documentation/publishing/transports/durable]>, the message could be executed by any running node within the system rather than the currently running process. If you aren't using durable messaging, the message is kept and scheduled in memory. Do be aware of that for the sake of memory usage and whether or not the scheduled execution should survive past the lifetime of the current process.





## Schedule Execution From Cascading Messages

To schedule a message to another system as a <[linkto:documentation/execution/cascading;title=cascading message]> from a message handler, 
you can return the `ScheduledResponse` object like this:

<[sample:DelayedResponseHandler]>


## RequeueLater

The `RetryLater()` mechanism in message error handling is using the scheduled execution. See <[linkto:documentation/execution/errorhandling]> for more information.

