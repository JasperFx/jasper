<!--title:Sql Server Transaction Middleware-->

Assuming that the Jasper.Persistence.SqlServer Nuget is referenced by your project, you can use the `[Transaction]` attribute on message (or HTTP) handler methods to wrap the message handling inside
a single Sql Server transaction like so:

<[sample:UsingSqlTransaction]>

When you use this middleware, be sure to pull in the current `SqlTransaction` object as a parameter to your handler method.