<!--title:Jasper.SqlServer-->

The Jasper.SqlServer Nuget library provides Jasper users with a quick way to integrate Sql Server-backed persistence into their
Jasper applications. To get started, just add the *Jasper.SqlServer* Nuget to your project, and enable the persistence like this:

<[sample:AppUsingSqlServer]>

Enabling this configuration adds a couple things to your system:

* Service registrations in your IoC container for `DbConnection` and `SqlConnection`, with the `Scoped` lifecycle
* <[linkto:documentation/extensions/sqlserver/outbox;title="Outbox" pattern usage with Sql Server]>
* Message persistence using your application's Sql Server database, including outbox support with Sql Server and <[linkto:documentation/messaging/transports/durable]> using Sql Server
* Support for <[linkto:documentation/extensions/sqlserver/transactions]>