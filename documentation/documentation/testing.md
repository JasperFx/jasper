<!--title:Automated Testing Support-->

TODO -- introduction


## Message Tracking

Automated testing against asynchronous processing applications can be very challenging. Let's say that when you're application handles a certain message it also sends out a couple cascading messages that in turn cause changes in state to the system
that you want to verify in your automated tests. The tricky part is how to invoke the original message, but then waiting for the cascading operations to complete before letting the test harness proceed to verifying the system state. Using `Thread.Sleep()` is *one* alternative, but usually results in either unnecessarily slow or horrendously unreliable automated
tests.

To at least ameliorate the issues around timing, Jasper comes with the "Message Tracking" feature that can be used as a helper in automated testing. To enable that in your applications, just include the extension as shown below:

<[sample:AppUsingMessageTracking]>

Now, in testing you can use extension methods off of `IJasperHost` that will execute an action with the service bus and 
wait until all the work started (messages sent should be received, cascading messages should be completed, etc.) has completed --
or it times out in a reasonable time.

To use the message tracking, consider this skeleton of a test:

<[sample:invoke_a_message_with_tracking]>

The other usages of message tracking are shown below:

<[sample:other-message-tracking-usages]>

TODO -- show how to see what came out
TODO -- show many more of the usages


## Message Tracking across Systems

TODO --> talk more about here

## Stubbing Outgoing External Transports

TODO --> talk more about here


## Integration with Storyteller

Jasper comes with a pre-built recipe for doing integration or acceptance testing with [Storyteller](http://storyteller.github.io) using
the *Jasper.TestSupport.Storyteller* extension library.

To get started with this package, create a new console application in your solution and add the `Jasper.TestSupport.Storyteller` Nuget dependency. Next,
in the `Program.Main()` method, use this code to connect your application to Storyteller:

<[sample:bootstrapping-storyteller-with-Jasper]>

In this case, `MyJasperAppRegistry` would be the name of whatever the `JasperRegistry` class is for your application.

If you want to hook into events during the Storyteller bootstrapping, teardown, or specification execution, you can subclass `JasperStorytellerHost<T>` like this:

<[sample:MyJasperStorytellerHarness]>

Then, your bootstrapping changes slightly to:

<[sample:running-MyJasperStorytellerHarness]>

## MessagingFixture

Jasper.Storyteller also comes with a `MessagingFixture` base class you can use to create Storyteller Fixtures that send messages to the running service bus with some facility to use
the built in <[linkto:documentation/testing/message_tracking]> to "know" when all the activity 
related to the message being sent has completed.

Here's a sample `MessagingFixture` from the sample project:

<[sample:TeamFixture]>

## Diagnostics

If there is any messages sent or received by the service bus feature during a Storyteller specification, there will be a custom results tab called "Messages" in the Storyteller
specification results that presents information about the message activity that will
look like this:

<[img:content/storyteller-messaging-log.png]>


## External Nodes

<[warning]>
This isn't super duper mature yet. We had a similar feature in FubuMVC that we'd like to have back, so look for more here later.
<[/warning]>

It's not for the feint of heart, but it's also possible to write automated tests using Storyteller against additional
systems for true integration testing.

To bootstrap an additional Jasper application in the same Storyteller host, use the "external nodes" feature like this:

<[sample:adding-external-node]>

If you're using `MessagingFixture`, you'll have access to the external nodes as shown in this fixture:

<[sample:IncrementFixture]>


