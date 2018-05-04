<!--title:Outbox Pattern with Marten-->

See [Jasper’s “Outbox” Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/) for more context around why you would care about the "outbox" pattern.

Jasper supports the ["outbox" pattern](https://jimmybogard.com/refactoring-towards-resilience-evaluating-coupling/) with [Marteh](https://jasperfx.github.io/marten) sessions. You can explicitly opt into this usage with code like this:

<[sample:ExplicitOutboxUsage]>

Do note that with the Marten-backed outbox usage, Jasper is able to listen for `IDocumentSession.SaveChanges()` or `IDocumentSession.SaveChangesAsync()` and flush out the persisted, outgoing messages into the actual sending queues. There is
no need to do anything else for the messages to be sent out to other services.

If you use the `[MartenTransaction]` middleware in a message handler, the middleware will take care of some of the repetitive mechanics for you. In the code below, the `IMessageContext` behind the scenes of the message being currently handled is enrolled in the current transaction represented by the current Marten `IDocumentSession` before the action runs, and the outgoing messages
are flushed into the outgoing sending queue after the action runs.

<[sample:MartenCreateItemHandler]>

See also <[linkto:documentation/extensions/sqlserver/outbox]>