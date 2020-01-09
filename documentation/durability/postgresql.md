<!--title:Using Postgresql with Jasper-->

The Jasper.Persistence.Postgresql Nuget library provides Jasper users with a quick way to integrate Postgresql-backed persistence into their
Jasper applications. To get started, just add the *Jasper.Persistence.Postgresql* Nuget to your project, and enable the persistence like this:

<[sample:AppUsingPostgresql]>

Enabling this configuration adds a couple things to your system:

* Service registrations in your IoC container for `DbConnection` and `NpgsqlConnection`, with the `Scoped` lifecycle
* "Outbox" pattern support as demonstrated below
* Message persistence using your application's Postgresql database, including outbox support with Postgresql
* Support for the `[Transactional]` attribute as shown below

## Transactional Middleware

Assuming that the Jasper.Persistence.Postgresql Nuget is referenced by your project, you can use the `[Transactional]` attribute on message (or HTTP) handler methods to wrap the message handling inside
a single Sql Server transaction like so:

<[sample:UsingNpgsqlTransaction]>

When you use this middleware, be sure to pull in the current `NpgsqlTransaction` object as a parameter to your handler method.

 ## "Outbox" Pattern Usage

 See [Jasper’s “Outbox” Pattern Support](https://jeremydmiller.com/2018/04/16/jaspers-outbox-pattern-support/) for more context around why you would care about the "outbox" pattern.

Jasper supports the ["outbox" pattern](https://jimmybogard.com/refactoring-towards-resilience-evaluating-coupling/) with Postgresql connections. You can explicitly opt into this usage with code like this:

<[sample:basic-postgresql-outbox-sample]>

If you use the `[Transaction]` middleware in a message handler, the middleware will take care of some of the repetitive mechanics for you. In the code below, the `IMessageContext` is enrolled in the current transaction before the action runs, and the outgoing messages
are flushed into the outgoing sending queue after the action runs.

<[sample:PostgresqlOutboxWithNpgsqlTransaction]>

## Message Persistence Schema

The message persistence requires and adds these tables to your schema:

1. `jasper_incoming_envelopes` - stores incoming and scheduled envelopes until they are successfully processed
1. `jasper_outgoing_envelopes` - stores outgoing envelopes until they are successfully sent through the transports
1. `jasper_dead_letters` - stores "dead letter" envelopes that could not be processed. See <[linkto:execution/dead_letter_queue]> for more information

## Managing the Postgresql Schema

In testing, you can build -- or rebuild -- the message storage in your system with a call to the `RebuildMessageStorage() ` extension method off of either `IWebHost` or `IJasperHost` as shown below in a sample taken from xUnit integration testing with Jasper:

<[sample:MyJasperAppFixture]>

See [this GitHub issue](https://github.com/JasperFx/jasper/issues/372) for some utilities to better manage the database objects.