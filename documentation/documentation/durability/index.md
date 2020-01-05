<!--title:Durable Messaging and Command Processing-->


Jasper supports durable message persistence using your application's database for "[store and forward](https://en.wikipedia.org/wiki/Store_and_forward)" queueing with all possible Jasper transport options, including the built in <[linkto:documentation/messaging/transports/durable]>, <[linkto:documentation/messaging/transports/rabbitmq]>. and <[linkto:documentation/messaging/transports/azureservicebus]>.


If a Jasper system that uses durable messaging goes down before all the messages are processed, the persisted messages will be loaded from
storage and processed when the system is restarted. Jasper does not include any kind of persistence in the core Jasper library, so you'll have to use
an extension library to add that behavior. Today the options are:

1. A <[linkto:documentation/extensions/marten/persistence;title=Marten/Postgresql backed option]>
1. A <[linkto:documentation/extensions/sqlserver/persistence;title=Sql Server backed option]>

With an option [based on EF Core planned for later](https://github.com/JasperFx/jasper/issues/363).


To use the built in <[linkto:documentation/messaging/transports/tcp]> in a durable way, just use the schema *durable* instead of *tcp* like so:

<[sample:DurableTransportApp]>


See the blog post [Durable Messaging in Jasper](https://jeremydmiller.com/2018/02/06/durable-messaging-in-jasper/) for more context behind the durable messaging.



## Message Storage in Testing

Let's say that we're all good developers who invest in automated testing of our applications. Now, let's say that we're building a Jasper application that uses Sql Server backed message persistence like so:

<[sample:SqlServerPersistedMessageApp]>

If we write integration tests for our application above, we need to guarantee that as part of the test setup the necessary Sql Server schema objects
have been created in our test database before we run any tests. 

Fortunately, Jasper comes with an extension method hanging off of `IJasperHost` called `RebuildMessageSchema()` that will completely rebuild all the necessary schema objects for message persistence. Below is an example of using an [xUnit shared fixture](https://xunit.github.io/docs/shared-context) approach for integration tests of the `MyJasperApp` application.

<[sample:MyJasperAppFixture]>


## Message Storage from the Command Line

As of Jasper v0.9.5, Jasper comes with a built in command for adminstering database backed persistence. Assuming that you're using <[linkto:documentation/bootstrapping/console;title=Jasper's command line support]>, you have the command `storage` with several options.

At the command line in the root of your application, you can rebuild the message storage schema objects with:

```
dotnet run -- storage rebuild
```

You can also query the current counts of persisted input, output, and scheduled messages with:

```
dotnet run -- storage counts
```

You can dump the SQL to create the necessary database objects to a file for usage in database migration scripts with:

```
dotnet run -- storage script --file SomeFilePath.sql
```

And lastly, if you just want to clear out any persisted incoming, outgoing, or scheduled messages in your application's database, use:

```
dotnet run -- storage clear
```

## Durable Messaging to External Systems

To utilize Jasper's durable messaging support and associated outbox support with other external systems, just utilize a Jasper message handler to do the actual integration with the external system. For example, you can send messages to an external web service by making the `HttpClient` call inside of a Jasper message handler through a durable <[linkto:documentation/messaging/transports/loopback;title=loopback queue]>.