<!--title:Working Directly with Envelopes-->

The `IMessageContext.Send(message, Action<Envelope>)` method allows you to override how a Jasper message is sent and even processed by directly altering the `Envelope`:

<[sample:CustomizingEnvelope]>