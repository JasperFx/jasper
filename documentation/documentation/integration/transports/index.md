<!--title:Messaging Transports-->

A *transport* is a queueing or communication mechanism that Jasper knows how to use to both deliver and receive messages to
and from other systems. The *transport* can take care of mapping your message objects and the Jasper `Envelope` to each transport's 
particular usage. Using Jasper should mostly make the choice of underlying transport be opaque to your application code (except for the bootstrapping of course). That being said though, Jasper tries to enable as much of the advanced usage of your underlying transport
as possible.

## Configuring Listener or Sending Endpoints

All of the transport types can be configured by using meaningful `Uri` values specific to each transport type. You'll probably find it easier to use the specific configuration helpers for each transport type, but let's look at the generic Uri configuration mechanism first.

All listener or sender endpoints are registered from the `JasperOptions.Endpoints` property like this:

snippet: sample_SenderAndListener

It is important to note that all of the transport endpoint types can be used in either a lightweight [fire and forget](http://www.enterpriseintegrationpatterns.com/patterns/conversation/FireAndForget.html) with limited retries, or with durable, [store and forward messaging](https://en.wikipedia.org/wiki/Store_and_forward) or [guaranteed delivery](http://www.enterpriseintegrationpatterns.com/patterns/messaging/GuaranteedMessaging.html). For durable messaging.

For more information about specific transports, see:

<[TableOfContents]>



