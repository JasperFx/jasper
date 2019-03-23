<!--title:Jasper.Persistence.Postgresql-->

The Jasper.Persistence.Postgresql Nuget library provides Jasper users with a quick way to integrate Postgresql-backed persistence into their
Jasper applications. To get started, just add the *Jasper.Persistence.Postgresql* Nuget to your project, and enable the persistence like this:

<[sample:AppUsingPostgresql]>

Enabling this configuration adds a couple things to your system:

* Service registrations in your IoC container for `DbConnection` and `NpgsqlConnection`, with the `Scoped` lifecycle
* <[linkto:documentation/extensions/postgresql/outbox;title="Outbox" pattern usage with Postgresql]>
* Message persistence using your application's Postgresql database, including outbox support with Postgresql and <[linkto:documentation/messaging/transports/durable]> using Postgresql
* Support for <[linkto:documentation/extensions/postgresql/transactions]>