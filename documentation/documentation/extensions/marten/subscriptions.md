<!--title:Marten Backed Subscription Storage-->

You can opt into Marten-backed subscription storage by including this extension:

<[sample:AppWithMartenBackedSubscriptions]>

By default, this subscription persistence is expecting the database connection string to be in
the *marten_subscription_database* environment variable, but you can override that or anything
in the Marten configuration for **only the subscription storage** like this:

<[sample:AppUsingMartenSubscriptions]>

See <[linkto:documentation/messaging/routing/subscriptions]> for more information.

This extension will also enable Marten-backed <[linkto:documentation/messaging/nodes]> as well. The service node
information is stored as [Marten](https://jasperfx.github.io/marten) documents.


