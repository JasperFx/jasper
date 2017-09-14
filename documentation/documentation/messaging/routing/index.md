<!--title:Routing Messages-->

When a message is published without any explicit destination, Jasper evaluates its own routing rules to determine
where the message should be delivered and with what representation according to the receiving application's capabilities.

The routing can be done simply by using <[linkto:documentation/messaging/routing/static_routing]> that require your application
to know where the messages should go, or by using <[linkto:documentation/messaging/routing/subscriptions]> where your application
will determine subscribers for any message type. Lastly, you can control how Jasper behaves when it is not able to determine where to
send a message with <[linkto:documentation/messaging/routing/no_routing]>.
