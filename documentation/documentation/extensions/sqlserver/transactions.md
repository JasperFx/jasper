<!--title:Sql Server Transaction Middleware-->

You can use the `[SqlTransaction]` attribute on message (or HTTP) handler methods to wrap the message handling inside
a single Sql Server transaction like so:

<[sample:UsingSqlTransaction]>

When you use this middleware, be sure to pull in the current `SqlTransaction` object as a parameter to your handler method.