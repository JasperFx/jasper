<!--title:Marten Backed Subscription Storage-->

You can opt into Marten-backed subscription storage by including this extension:

<[sample:AppWithMartenBackedSubscriptions]>

By default, this subscription persistence is expecting the database connection string to be in
the *marten_subscription_database* environment variable, but you can override that or anything
in the Marten configuration for **only the subscription storage** like this:

<[sample:AppUsingMartenSubscriptions]>

See <[linkto:documentation/messaging/routing/subscriptions]> for more information.
