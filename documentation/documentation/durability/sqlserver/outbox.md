<!--title: Outbox Usage with Sql Server-->

See [Jasper’s “Outbox” Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/) for more context around why you would care about the "outbox" pattern.

Jasper supports the ["outbox" pattern](https://jimmybogard.com/refactoring-towards-resilience-evaluating-coupling/) with Sql Server connections. You can explicitly opt into this usage with code like this:

<[sample:basic-sql-server-outbox-sample]>

If you use the `[Transaction]` middleware in a message handler, the middleware will take care of some of the repetitive mechanics for you. In the code below, the `IMessageContext` is enrolled in the current transaction before the action runs, and the outgoing messages
are flushed into the outgoing sending queue after the action runs.

<[sample:SqlServerOutboxWithSqlTransaction]>

