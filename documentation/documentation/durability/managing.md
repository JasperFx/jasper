<!--title:Message Storage Management-->

## From the Command Line

As of Jasper v0.9.5, Jasper comes with a built in command for adminstering database backed persistence. Assuming that you're using <[linkto:documentation/console;title=Jasper's command line support]>, you have the command `storage` with several options.

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



## Message Storage in Testing

Let's say that we're all good developers who invest in automated testing of our applications. Now, let's say that we're building a Jasper application that uses Sql Server backed message persistence like so:

<[sample:SqlServerPersistedMessageApp]>



If we write integration tests for our application above, we need to guarantee that as part of the test setup the necessary Sql Server schema objects have been created in our test database before we run any tests. If you notice in the code above, there's a property called 
`JasperOptions.Advanced.StorageProvisioning` that is defaulted to `None`, but can be overridden to either `Clear` to delete any persisted messages on application startup or `Rebuild` to completely drop and rebuild all message persistence storage objects in the database upon
application startup. 

In addition to the `StorageProvisioning` property, there is also an extension method hanging off of `IJasperHost` called `RebuildMessageSchema()` that will completely rebuild all the necessary schema objects for message persistence. Below is an example of using an [xUnit shared fixture](https://xunit.github.io/docs/shared-context) approach for integration tests of the `MyJasperApp` application.

<[sample:MyJasperAppFixture]>


