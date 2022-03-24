<!--title:Publishing and Sending -->

Publish/Subscribe is a messaging pattern where the senders of messages do not need to specifically know what the specific subscribers are for a given message. In this case, some kind of middleware or infrastructure is responsible for either allowing subscribers to express interest in what messages they need to receive or apply routing rules to send the published messages to the right places. Jasper's messaging support was largely built to support the publish/subscibe messaging patterm.

To send a message with Jasper, use the `IMessagePublisher` interface or the bigger `IMessageContext` interface that
is registered in your application IoC container. The sample below shows the most common usage:

snippet: sample_sending_message_with_servicebus

That by itself will send the `InvoiceCreated` message to whatever subscribers are interested in
that message. The `Send()` method will throw an exception if Jasper doesn't know where to send the message. In other words,
there has to be a subscriber of some sort for that message.

On the other hand, the `Publish()` method will send a message if there is a known subscriber and ignore the message if there is
no subscriber:

snippet: sample_publishing_message_with_servicebus

To take more control over how a message is sent, you can work directly with the Jasper <[linkto:documentation/integration/customizing_envelopes;title=Envelope]>.
