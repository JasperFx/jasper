<!--title:Static Publishing Rules-->

When you publish a message using `IServiceBus` without explicitly setting the Uri of the desired 
destination, Jasper has to invoke the known message routing rules and dynamic subscriptions to
figure out which locations should receive the message. Consider this code that publishes a
`PingMessage`:

<[sample:sending-messages-for-static-routing]>

To route `PingMessage` to a channel, we can apply static message routing rules by using one of the 
_SendMessage****_ methods as shown below:

<[sample:StaticRoutingApp]>

Do note that doing the message type filtering by namespace will also include child namespaces. In
our own usage we try to rely on either namespace rules or by using shared message assemblies. 

See also the <[linkto:documentation/messaging/routing/subscriptions]>


