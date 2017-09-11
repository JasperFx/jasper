<!--title: Messaging-->

Jasper's support for messaging is a direct descendent of the <a href="https://fubumvc.github.io/documentation/servicebus/">service bus feature in FubuMVC</a>, which in turn
was heavily influenced and built as a replacement for the <a href="https://hibernatingrhinos.com/oss/rhino-service-bus">Rhino Service Bus</a>. The messaging feature in Jasper allows developers to quickly and reliably implement common messaging patterns like publish/subscribe or request/reply in their applications.

For more information, see:

<[TableOfContents]>

## Terminology

* _Node_ - a running instance of a Jasper application. Do note that it's actually possible to run more than
  one Jasper application in the same process
* _Transport_ - a supported mechanism for sending messages between running Jasper nodes
* _Envelope_ - an object that wraps a message being sent or received by JasperBus that adds header metadata and helps control
  how messages are sent and received. See the [Envelope Wrapper pattern](http://www.enterpriseintegrationpatterns.com/patterns/messaging/EnvelopeWrapper.html) for more information
* _Publish/Subscribe_ (pubsub) - messaging pattern that decouples the sending code from the routing to one or more recipients. One way communication from the sender
  to any receivers. See [Publish-Subscribe Channel](http://www.enterpriseintegrationpatterns.com/patterns/messaging/PublishSubscribeChannel.html) for more background.
* _Request/Reply_ - bi-directional messaging pattern where a request message to one node generates a response message back to the original sender
* _Frame's_ - Jasper's middleware strategy
* _Handler's_ - a class that handles messages within a Jasper service bus application

