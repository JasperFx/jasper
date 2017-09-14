<!--title:Messaging Transports-->

A *transport* is a queueing or communication mechanism that Jasper knows how to use to both deliver and receive messages to
and from other systems. In usage, you generally supply a `Uri` to the message routing, subscription, or listening configuration
that the matching transport can interpret. For example, the <[linkto:documentation/messaging/transports/lightweight;title=lightweight transport]>
accepts `Uri's` like *tcp://remoteserver:2200* that is interpreted by Jasper as:

1. Using the built in, lightweight fire and forget transport...
1. Send messages to the DNS entry for *remoteserver*...
1. Using port 2200 at the remote server (or docker image or VM or load balancer or whatever)

Jasper comes out of the box with these transports:

* <[linkto:documentation/messaging/transports/lightweight]> associated with the "tcp" scheme in Uri definitions
* <[linkto:documentation/messaging/transports/durable]> associated with the "durable" scheme in Uri definitions
* <[linkto:documentation/messaging/transports/loopback]> associated with the "loopback" scheme in Uri definitions
* <[linkto:documentation/messaging/transports/http]> - **Forthcoming**

Transports based on [RabbitMQ](https://www.rabbitmq.com/) and/or [Azure Service Bus](https://azure.microsoft.com/en-us/services/service-bus/) are part of the Jasper roadmap.


## Disable All Transports

You might very well want to start up a Jasper application with all of the transports disabled. Maybe it's for testing scenarios
where the necessary transport infrastructure isn't in place or you want a faster bootstrap time. Regardless, it's simply this syntax:

<[sample:TransportsAreDisabled]>

