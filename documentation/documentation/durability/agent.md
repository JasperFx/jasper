<!--title:Durability Agent-->

If you add one of the database-backed message persistence providers to your Jasper application, Jasper will run a `DurabilityAgent` in the background of your application (utilizing .Net Core's [IHostedService](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-3.1&tabs=visual-studio) functionality). If you keep the logging level all the way down to *Debugging*, you'll see a great deal of tracing from this process. The `DurabilityAgent` is polling your application database looking for:

1. Persisted messages that were scheduled later for execution or sending that are ready to out
1. Moving message ownership over from nodes that appear to be offline
1. Recovering persisted, outgoing messages that are not actively owned by any running node and sending them on their way
1. Recovering persisted, incoming messages that are not actively owned by any running node and executing them in the local node

At this point, Jasper relies very heavily on [advisory locks in Postgresql](https://www.postgresql.org/docs/12/explicit-locking.html) or 
[application locks in Sql Server](https://www.oreilly.com/library/view/microsoft-sql-server/9781118282175/c47_level1_8.xhtml) to coordinate activities across running nodes of the same system.

There are a few configuration items of the `DurabilityAgent` you might want to tweak in your system:

<[sample:AdvancedConfigurationOfDurabilityAgent]>