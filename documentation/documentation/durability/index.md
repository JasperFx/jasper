<!--title:Durable Messaging and Command Processing-->

One of Jasper's most important features is durable message persistence using your application's database for reliable "[store and forward](https://en.wikipedia.org/wiki/Store_and_forward)" queueing with all possible Jasper transport options, including the lightweight <[linkto:documentation/integration/transports/tcp]> and external transports like <[linkto:documentation/integration/transports/rabbitmq]> or and <[linkto:documentation/integration/transports/azureservicebus]>.

It's a chaotic world out when high volume systems need to interact with other systems. Your system may fail, other systems may be down,
there's network hiccups, occasional failures -- and you still need your systems to get to a consistent state without messages just
getting lost en route. 

To that end, Jasper relies on message persistence within your application database as it's implementation of the [Transactional Outbox](https://microservices.io/patterns/data/transactional-outbox.html) pattern. Using the "outbox" pattern is a way to avoid the need for problematic
and slow [distributed transactions](https://en.wikipedia.org/wiki/Distributed_transaction) while still maintaining eventual consistency between database changes and the outgoing messages that are part of the logical transaction. Jasper implementation of the outbox pattern
also includes a separate *message relay* process that will send the persisted outgoing messages in background processes (it's done by marshalling the outgoing message envelopes through [TPL Dataflow](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/dataflow-task-parallel-library) queues if you're curious.)


If a Jasper system that uses durable messaging goes down before all the messages are processed, the persisted messages will be loaded from
storage and processed when the system is restarted. Jasper does not include any kind of persistence in the core Jasper library, so you'll have to use
an extension library to add that behavior. 

Today the options are:

1. A <[linkto:documentation/durability/postgresql;title=Postgresql backed option]> 
1. A <[linkto:documentation/durability/sqlserver;title=Sql Server backed option]>

<[info]>
You will need to be using some kind of database-backed message persistence in order to make the <[linkto:documentation/integration/scheduled]> or <[linkto:documentation/local;title=scheduled local execution]> function durably.
<[/info]>

Note that the <[linkto:documentation/durability/marten]> support relies on the Postgresql backed persistence, and the 
<[linkto:documentation/durability/efcore]> support will need to be used in conjunction with either the Sql Server or Postgresql backed 
message persistence.

The message durability also applies to the <[linkto:documentation/local;title=local worker queues]>.

Also see the following topics to learn more about using, managing, and configuring the durable message persistence:

<[TableOfContents]>