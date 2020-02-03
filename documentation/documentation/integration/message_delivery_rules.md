<!--title:Message Type Specific Delivery Rules-->

You may want to enforce some specific rules about how Jasper publishes messages on a message type by message
type basis. The most common example is shown below:

<[sample:UsingDeliverWithinAttribute]>

## Custom Attributes

If you really want to, you can write your own custom attribute to modify how Jasper sends out a message
by subclassing the `[ModifyEnvelope]` attribute. The attribute shown in the previous sample is itself
implemented like that:

<[sample:DeliverWithinAttribute]>

## By Endpoint

You may want to say that all the envelopes sent to a specific endpoint should have the same customization. As an example,
let's say that you're sending rapid fire status messages to some kind of monitoring tool where you're not terribly worried about any
particular message getting dropped and each individual message will soon be obsolete. In this case you might want to set
a message expiration date on every message sent to this endpoint. You can do that with endpoint specific rules like this:

<[sample:MonitoringDataPublisher]>



