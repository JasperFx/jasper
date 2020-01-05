<!--title:Postgresql Transaction Middleware-->

Assuming that the Jasper.Persistence.Postgresql Nuget is referenced by your project, you can use the `[Transactional]` attribute on message (or HTTP) handler methods to wrap the message handling inside
a single Sql Server transaction like so:

<[sample:UsingNpgsqlTransaction]>

When you use this middleware, be sure to pull in the current `NpgsqlTransaction` object as a parameter to your handler method.