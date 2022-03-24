<!--title:Automated Testing Support-->

Jasper was built intentionally with [testability](https://en.wikipedia.org/wiki/Software_testability) as a first class design goal.

## General Guidance

* For any kind of integration testing, the Jasper team suggests bootstrapping your Jasper application to an `IHost` in
  a test harness as closely to the production application setup as you can -- minus inconvenient external dependencies
* To isolate your Jasper application from any kind of external transports that you might not want to access locally, use the
  transport stubbing explained in a section below
* If your message handlers do not use any kind of Jasper middleware, it might be easy enough to simply resolve your handler class from the underlying IoC container. Use `IHost.Services.GetRequiredService<T>()` to resolve the handler objects, and Lamar is able to figure out a construction strategy on the fly without any kind of prior registration.
* If your message handlers do not involve any kind of cascading messages, use `ICommandBus.Invoke()` to execute a message inline
* If your message handler *does* involve cascading messages, use the message tracking support explained in the next session
* If you are trying to coordinate a test across multiple Jasper applications, see the section on *Message Tracking across Systems*





## Message Tracking

::: tip warning
The message tracking adds a little bit of extra overhead to the logging in Jasper, and should *not* be used
in production.
:::

Jasper is a successor to a much earlier project called [FubuMVC](https://fubumvc.github.io). One of the genuinely successful
parts of FubuMVC was a mechanism to coordinate automated testing of its messaging support as described in these blog posts:

* [Automated Testing of Message Based Systems](https://jeremydmiller.com/2016/05/16/automated-testing-of-message-based-systems/)
* [Reliable and “Debuggable” Automated Testing of Message Based Systems in a Crazy Async World](https://jeremydmiller.com/2016/05/17/reliable-and-debuggable-automated-testing-of-message-based-systems-in-a-crazy-async-world/)

Jasper's version of this feature is much improved from FubuMVC, and comes out of the box to make testing scenarios easier.


Automated testing against asynchronous processing applications can be very challenging. Let's say that when you're application handles a certain message it also sends out a couple cascading messages that in turn cause changes in state to the system
that you want to verify in your automated tests. The tricky part is how to invoke the original message, but then waiting for the cascading operations to complete before letting the test harness proceed to verifying the system state. Using `Thread.Sleep()` is *one* alternative, but usually results in either unnecessarily slow or horrendously unreliable automated
tests.

To at least ameliorate the issues around timing, Jasper comes with the "Message Tracking" feature that can be used as a helper in automated testing. To enable that in your applications, just include the extension as shown below:

snippet: sample_AppUsingMessageTracking


Now, in testing you can use extension methods off of `IHost` that will execute an action with the service bus and 
wait until all the work started (messages sent should be received, cascading messages should be completed, etc.) has completed --
or it times out in a reasonable time. **The message tracking will throw an exception if it times out without completing**, and the exception will list out all the detected activity to try to help trouble shoot where things went wrong.

To use the message tracking, consider this skeleton of a test:

snippet: sample_invoke_a_message_with_tracking

The other usages of message tracking are shown below:

snippet: sample_other_message_tracking_usages

All of these methods return an `ITrackedSession` object that gives you access to the tracked activity. Here's an example from Jasper's tests that uses `ITrackedSession`:

snippet: sample_using_stubbed_listeners

The `ITrackedSession` interface looks like this:

snippet: sample_ITrackedSession


## Message Tracking with External Transports

By default, the message tracking runs in a "local" mode that logs any outgoing messages to external transports, but doesn't wait for 
any indication that those messages are completely received on the other end. To include tracking of activity from external transports
even when you are testing one application, use this syntax shown in an internal Jasper test for the Azure Service Bus support:

snippet: sample_can_stop_and_start_ASB



## Stubbing Outgoing External Transports

Sometimes you'll need to develop on your Jasper application without your application having any access to external transports for one reason or another. Jasper still has you covered with the feature `JasperOptions.Endpoints.StubAllExternallyOutgoingEndpoints()` shown below:

snippet: sample_UseJasperWithInlineOptionsConfigurationAndHosting

When this is active, Jasper will simply not start up any of the configured listeners or subscribers for external transports. Any messages published to these endpoints will simply be ignored at runtime -- but you can still use the message tracking feature shown above to capture the outgoing messages for automated testing.


## Integration with Storyteller

Jasper comes with a pre-built recipe for doing integration or acceptance testing with [Storyteller](http://storyteller.github.io) using
the *Jasper.TestSupport.Storyteller* extension library.

To get started with this package, create a new console application in your solution and add the `Jasper.TestSupport.Storyteller` Nuget dependency. Next,
in the `Program.Main()` method, use this code to connect your application to Storyteller:

snippet: sample_bootstrapping_storyteller_with_Jasper

In this case, `MyJasperAppRegistry` would be the name of whatever the `JasperRegistry` class is for your application.

If you want to hook into events during the Storyteller bootstrapping, teardown, or specification execution, you can subclass `JasperStorytellerHost<T>` like this:

snippet: sample_MyJasperStorytellerHarness

Then, your bootstrapping changes slightly to:

snippet: sample_running_MyJasperStorytellerHarness

## MessagingFixture

Jasper.Storyteller also comes with a `MessagingFixture` base class you can use to create Storyteller Fixtures that send messages to the running service bus with some facility to use
the built in <[linkto:documentation/testing/message_tracking]> to "know" when all the activity 
related to the message being sent has completed.

Here's a sample `MessagingFixture` from the sample project:

snippet: sample_TeamFixture

## Diagnostics

If there is any messages sent or received by the service bus feature during a Storyteller specification, there will be a custom results tab called "Messages" in the Storyteller
specification results that presents information about the message activity that will
look like this:

<[img:content/storyteller-messaging-log.png]>
