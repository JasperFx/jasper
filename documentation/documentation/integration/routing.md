<!--title:Routing Messages-->

When you publish a message using `IMessageContext` without explicitly setting the Uri of the desired 
destination, Jasper has to invoke the known message routing rules and dynamic subscriptions to
figure out which locations should receive the message. Consider this code that publishes a
`PingMessage`:

snippet: sample_sending_messages_for_static_routing

To route `PingMessage` to a channel, we can apply static message routing rules by using the 
`Endpoint.Publish()` method as shown below:

snippet: sample_StaticPublishingRules

Do note that doing the message type filtering by namespace will also include child namespaces. In
our own usage we try to rely on either namespace rules or by using shared message assemblies. 
