<!--title:Durable Messaging-->

<div class="alert alert-warning"><b>Note!</b> This transport works by sending traffic directly via sockets and may not be acceptable in your IT department policies. We are pursuing the usage of JWT's to secure the traffic between applications using the socket based transports, see <a href="https://github.com/JasperFx/jasper/issues/184">the GitHub issue</a></div>

<div class="alert alert-info"><b>Note!</b> The durable transport is "wire compatible" with the lightweight transport, and it is possible to send from
a durable transport Uri in one application to a lightweight transport listener in a second application or vice versa.</div>

The durable transport is meant for scenarios where guaranteed delivery is required. The durable transport is a "[store and forward](https://en.wikipedia.org/wiki/Store_and_forward)" queueing mechanism
that uses some kind of pluggable persistence to track messages from the sender until they are successfully received by the intended subscriber,
and from the subscriber until the message has either been successfully processed or the error policies delete or move the message
after too many failures.

If a Jasper system that uses the durable transport goes down before all the messages are processed, the persisted messages will be loaded from
storage and processed when the system is restarted. Jasper does not include any kind of persistence in the core Jasper library, so you'll have to use
an extension library to add that behavior. Today the options are:

1. A [Marten/Postgresql backed option](documentation/extensions/marten/persistence)

1. Sql Server-backed message persistence is also planned, with specific "outbox" usage patterns for [Dapper](http://dapper-tutorial.net/dapper) and probably later
   [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/). Follow the [discussion on this planned feature on GitHub](https://github.com/JasperFx/jasper/issues/259).



To use the durable transport, here are examples of all the common use cases:

<[sample:DurableTransportApp]>

