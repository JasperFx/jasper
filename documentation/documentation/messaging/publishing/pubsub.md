<!--title:Publish / Subscribe -->

Publish/Subscribe is a messaging pattern where the senders of messages do not need to specifically know what the specific subscribers are for a given message. In this case, some kind of middleware or infrastructure is responsible for either allowing subscribers to express interest in what messages they need to receive or apply routing rules to send the published messages to the right places. Jasper's messaging support was largely built to support the publish/subscibe messaging patterm.

To send a message with Jasper, use the `IServiceBus` like this:

<[sample:sending-message-with-servicebus]>

That by itself will send the `InvoiceCreated` message to whatever subscribers are interested in
that message.

## Sending to a Specific Destination

You can bypass the routing and subscribing rules and designate the destination for a message like this:

<[sample:send-message-to-specific-destination]>
