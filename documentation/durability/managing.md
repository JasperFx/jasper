<!--title:Message Storage Management-->

## From the Command Line

As of Jasper v0.9.5, Jasper comes with a built in command for adminstering database backed persistence. Assuming that you're using <[linkto:console;title=Jasper's command line support]>, you have the command `storage` with several options.

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
