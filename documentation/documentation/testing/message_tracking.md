<!--title:Message Tracking-->

Automated testing against asynchronous processing applications can be very challenging. Let's say that when you're application handles a certain message it also sends out a couple cascading messages that in turn cause changes in state to the system
that you want to verify in your automated tests. The tricky part is how to invoke the original message, but then waiting for the cascading operations to complete before letting the test harness proceed to verifying the system state. Using `Thread.Sleep()` is *one* alternative, but usually results in either unnecessarily slow or horrendously unreliable automated
tests.

To at least amerliorate the issues around timing, Jasper comes with the "Message Tracking" feature that can be used as a helper in automated testing. To enable that in your applications, just include the extension as shown below:

<[sample:AppUsingMessageTracking]>

Now, in testing you can use extension methods off of `JasperRuntime` that will execute an action with the service bus and 
wait until all the work started (messages sent should be received, cascading messages should be completed, etc.) has completed --
or it times out in a reasonable time.

To use the message tracking, consider this skeleton of a test:

<[sample:invoke_a_message_with_tracking]>

The other usages of message tracking are shown below:

<[sample:other-message-tracking-usages]>
