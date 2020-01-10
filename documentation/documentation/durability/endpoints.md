<!--title:Adding Durability to Senders or Listeners-->

To add durable messaging behavior to any kind of endpoint (), use the `Durably()` method to tag any listener or publishing point as durable as shown below:

<[sample:DurableTransportApp]>

This sample uses the built in <[linkto:documentation/integration/transports/tcp]>, but the durability option is available for any supported
Jasper transport including the <[linkto:documentation/local]>.


See the blog post [Durable Messaging in Jasper](https://jeremydmiller.com/2018/02/06/durable-messaging-in-jasper/) for more context behind the durable messaging.

Consider this sample [Ping/Pong Jasper sample application](https://github.com/JasperFx/JasperSamples/tree/master/PingPong) that
uses the the lightweight, [fire and forget](https://www.enterpriseintegrationpatterns.com/patterns/conversation/FireAndForget.html) <[linkto:documentation/integration/transports/tcp]>. The sample application include a *Pinger* system that sends "ping" messages to a second *Ponger* system, which turns around and sends "pong" replies back to the original sender. Without any kind of message persistence, Jasper
can successfully send outgoing "ping" messages and the corresponding "pong" replies when both *Pinger* and *Ponger* are running and
available.

If you need [guaranteed delivery](https://www.enterpriseintegrationpatterns.com/patterns/messaging/GuaranteedMessaging.html) of your messages, you might want to opt into Jasper's durable messaging -- even if you're using an external messaging service like Rabbit MQ or
Azure Service Bus. 



To see the durable messaging in action, there are a pair of sample applications in GitHub that implement the very same *Ping/Pong* systems, but this time with durable messaging:

1. [Ping/Pong with Postgresql Backed Persistence](https://github.com/JasperFx/JasperSamples/tree/master/PingPongWithPostgresqlPersistence)
1. [Ping/Pong with Sql Server Backed Persistence](https://github.com/JasperFx/JasperSamples/tree/master/PingPongWithSqlServerPersistence)

In these examples, you can happily -- and randomly -- stop and start both *Pinger* and *Ponger* without any outbound messages getting lost. For any inbound messages that were received but not actually processed some how when the systems are shut down, you will also see
those messages get processed by the receiving application when it is restarted.