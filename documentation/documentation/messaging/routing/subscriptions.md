<!--Title:Dynamic Subscriptions-->
<!--Url:subscriptions-->

In more advanced usages of the Jasper service bus, you may want your application to be able to
subscribe to messages produced by a separate application without having to first configure
the other application. The _subscriptions_ feature is the mechanism to make this registration without having
to directly couple your Jasper applications to each other. 

The subscriptions support consists of a few pieces and concepts:

<[img:content/subscriptions.png]>

1. Your application *should* declare the published messages it wants to subscribe to
1. *Optionally*, you may declare the messages that your application publishes **strictly for the sake of diagnostics**
1. A subscription storage mechanism that persists all the message subscriptions
1. Jasper's internal message routing that uses the known subscriptions to decide where published messages should be sent 
1. *Optional* <[linkto:documentation/bootstrapping/console;title=command line tooling]> to publish, list, or validate the known subscriptions


## Subscription Storage

The default subscription storage is just an in memory model, so you'll almost certainly want to replace that with some sort of
durable storage mechanism if you're going to use subscriptions. To use a different subscription storage, you need an implementation
of the `ISubscriptionsRepository` interface that you can plug into the application services like so:

<[sample:SubscriptionStorageOverride]>

You can see a [sample implementation for Marten here](https://github.com/JasperFx/jasper/blob/master/src/Jasper.Marten/Subscriptions/MartenSubscriptionRepository.cs).

As of today, the Jasper community has working subscription storage based on <[linkto:documentation/extensions/consul;title=Consul]> and
<[linkto:documentation/extensions/marten/subscriptions;title=Marten]> on top of a Postgresql database. 


## Configurating Subscriptions

Subscriptions are configured in the `JasperRegistry` class that establishes your application. 

As an example, let's say you have an application that wants to subscribe to messages:

<[sample:configuring-subscriptions]>

A couple things to note from the sample above:

* The `Subscribe.At(Uri)` call isn't technically mandatory, but if it's left out, the subscription will be made
  to the local machine where the Jasper node is running. If you're running multiple nodes behind a hardware load
  balancer or using a clustered queue like RabbitMQ, you'll want the subscription made to the load balancer.
* When Jasper makes the subscription, it also gathers up all the message versions and representations that your application
  knows how to read. As long as the publishing application can write to one of those representations and supports the transport
  in your call to `Subscribe.At(Uri)`, you should be good to go. See the section below on validating subscriptions.
* Your Jasper application **does not automatically publish its subscriptions on application startup**. This is a change from its
  FubuTransportation antecedent. See the section below about publishing subscriptions.


## Publishing Subscriptions

Assuming that you are using the `Jasper.CommandLine` package to <[linkto:documentation/bootstrapping/console;title=run your Jasper application]>, 
you'll have an extra command to publish all the subscriptions to the registered subscription storage. Assuming that 
your application compiles to "MyApp.exe," the command line would be:

```
|> MyApp subscriptions publish
```

If you just want to export the subscriptions to a JSON file where you can view the data that would be published to the
subscription storage, use:

```
|> MyApp subscriptions export --directory ~/subscriptions
```

where the `--directory / -d` flag just denotes where you want the file written. assuming that the 
service name is *MyApp*, the file exported will be `MyApp.capabilities.json`.

To just list the subscriptions in the console, it's:

```
|> MyApp subscriptions list
```


## Declaring Published Messages

Strictly for informational or diagnostic purposes, you can explicitly declare which message types your application
publishes like this:

<[sample:PublisherApp]>

Or if you're okay with this approach, you can use the `[Publish]` attribute in Jasper like so:

<[sample:using-[Publish]]>

**Do note that you can send message types even if that message type is not declared as being published.**

## Validating Subscriptions

The command line support also has the ability to scan through exported "capabilities" files from the `MyApp subscriptions export` command
and report and validate against the entire known ecosystem. Assuming that every related application exports their capabilities (requested subscriptions and known published messages) to a common directory or source control repository, you can use this command:

```
|> MyApp subscriptions validate --file subscription-report.json
```

In the sample above, we're writing the whole *messaging graph* of matched routes and potential problems to a file named
`subscription-report.json`.

Do note that this command will fail at the command line if it detects any problems unless you also use the `--ignore-failures` flag. Definitely use this flag if you are only using this command for informational purposes or you aren't being strict about
marking published messages.

This command compares publisher and subscription capabilities based on supported transports and message representations. This report can detect:

* Valid message routes for each permutation of message type, subscriber, and publisher 
* Messages referenced in subscriptions that have no known publisher
* Messages that are marked as published but not subscribed to by any known application
* Mismatched subscriptions based on publisher and subscriber capabilities. This will catch any issue with
  having no compatible message representations or matching transports


See <[linkto:documentation/messaging/messages]> for more information.
