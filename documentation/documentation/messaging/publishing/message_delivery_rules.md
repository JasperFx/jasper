<!--title:Message Type Specific Delivery Rules-->

You may want to enforce some specific rules about how Jasper publishes messages on a message type by message
type basis. The most common example is shown below:

<[sample:UsingDeliverWithinAttribute]>

## Using the JasperRegistry Fluent Interface

If you don't care for attributes or don't own the message type, there's also an equivalent mechanism
in the `JasperRegistry` fluent interface to define the same kind of `Envelope` customizations:

<[sample:StatusMessageSendingApp]>

## Custom Attributes

If you really want to, you can write your own custom attribute to modify how Jasper sends out a message
by subclassing the `[ModifyEnvelope]` attribute. The attribute shown in the previous sample is itself
implemented like that:

<[sample:DeliverWithinAttribute]>

