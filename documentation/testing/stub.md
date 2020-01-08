<!--title:"Stub" Transport for Testing-->

So here's a pretty common scenario, you have a Jasper application that sends messages through external transports to other applications, but you'd also like to author integration tests against your system in isolation without having to have all that external transport infrastructure. You'd probably also like to verify the messages your system is publishing. You're in luck, because Jasper supports a "stub" transport you can use during testing just to track the outgoing messages without actually sending them to a real queueing system. 

The stub transport is automatically registered and ready to go (as of v0.9.8.1), and its Uri pattern is just *stub://[queue name]*.

Let's say you've got a Jasper application that receives messages from one transport endpoint, and publishes all its outgoing messages to another transport endpoint:

<[sample:StubTransport-MyJasperApp]>

In real production, that application might have an appsettings.json file like so:

```
{
    "RabbitMq": {
        "rabbit": "some connection string"
    },
    "outgoing": "rabbitmq://rabbit/queue/outgoing",
    "incoming": "rabbitmq://rabbit/queue/incoming"
}
```

<[info]>
All the methods shown in these code samples for clearing or retrieving messages received by the Stub transport are 
extension methods in the *Jasper.Messaging.Transports.Stub* namespace. There are matching extension methods for both
Jasper's more expansive `IJasperHost` and the built in ASP.Net Core `IWebHost` interface, so you're covered no matter how you prefer to bootstrap your Jasper application.
<[/info]>

In local only integration testing, you're just going to want to override the connection settings for *incoming* and *outgoing* with Uri strings pointing to the "stub" transport. Assuming that you're using [xUnit.Net](https://xunit.github.io) as your test framework, you would probably want to bootstrap your application in a shared class fixture like this:

<[sample:StubTransport-IntegrationFixture]>

And maybe a reusable base class for integration tests like so:

<[sample:StubTransport-IntegrationContext]>

Now, your test fixture classes can inherit from this base class that uses the Stub transport to 
verify the expected messages sent:

<[sample:StubTransport-test-fixture-class]>

