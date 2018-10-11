<!--title:Messaging Transports-->

A *transport* is a queueing or communication mechanism that Jasper knows how to use to both deliver and receive messages to
and from other systems. In usage, you generally supply a `Uri` to the message routing, subscription, or listening configuration
that the matching transport can interpret. For example, the <[linkto:documentation/messaging/transports/tcp]>
accepts `Uri's` like *tcp://remoteserver:2200* that is interpreted by Jasper as:

1. Using the built in, TCP transport...
1. Send messages to the DNS entry for *remoteserver*...
1. Using port 2200 at the remote server (or docker image or VM or load balancer or whatever)

Jasper comes out of the box with these transports:

* <[linkto:documentation/messaging/transports/tcp]> associated with the "tcp" scheme in Uri definitions
* <[linkto:documentation/messaging/transports/loopback]> associated with the "loopback" scheme in Uri definitions
* <[linkto:documentation/messaging/transports/http]> associated with the "http" scheme that just accepts message batches through an ASP.Net Core route

There is also an addon for a <[linkto:documentation/messaging/transports/rabbitmq]> based on [RabbitMQ](https://www.rabbitmq.com/).

Additional transports based on [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) 
and possibly [Kafka](https://kafka.apache.org/) are part of the Jasper roadmap.

It is important to note that all of the transport types can be used in either a lightweight [fire and forget](http://www.enterpriseintegrationpatterns.com/patterns/conversation/FireAndForget.html) with limited retries, or with durable, [store and forward messaging](https://en.wikipedia.org/wiki/Store_and_forward) or [guaranteed delivery](http://www.enterpriseintegrationpatterns.com/patterns/messaging/GuaranteedMessaging.html). For durable messaging.

See <[linkto:documentation/messaging/transports/durable]> for more information about durable messaging support in Jasper.


## Disable All Transports

You might very well want to start up a Jasper application with all of the transports disabled. Maybe it's for testing scenarios
where the necessary transport infrastructure isn't in place or you want a faster bootstrap time. Regardless, it's simply this syntax:

<[sample:TransportsAreDisabled]>

## Disable Selected Transports

If you want to selectively disable some of the built in transport types, you can use this syntax:

<[sample:DisableIndividualTransport]>

A couple notes first though:

* The tcp transport is enabled by default
* Disabling a transport prevents Jasper from creating either outgoing
  channels or listeners for that transport



