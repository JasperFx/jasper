<!--title:Customizing the Sent Message Envelopes-->

The `IMessageContext.Send(message, Action<Envelope>)` method allows you to override how a Jasper message is sent and even processed by directly altering the `Envelope`:

<[sample:CustomizingEnvelope]>