<!--title:Configuring the Service Bus-->



The message bus capabilities of Jasper can be configured in a mix of ways:

1. Through your application configuration file
1. Directly through the `JasperOptions` model
1. `JasperOptionsBuilder` or `JasperRegistry`, which ultimately are just fluent interface wrappers to just configure the underlying `JasperOptions` model



## Using Your Configuration File

If you want to, you can completely configure the `JasperOptions` model in your `appsettings.json` file with a section called *Jasper* that would look like this:

<pre>
{
  "Jasper":{
    "HostedServicesEnabled": true,
    "DisableAllTransports": true,
    "ThrowOnValidationErrors": false,
    "Retries": {
      "Cooldown": "00:00:05",
      "FailuresBeforeCircuitBreaks": 3,
      "MaximumEnvelopeRetryStorage": 100,
      "RecoveryBatchSize": 100,
      "NodeReassignmentPollingTime": "00:01:00",
      "FirstNodeReassignmentExecution": "00:00:00"
    },
    "ScheduledJobs": {
      "FirstExecution": "00:00:00",
      "PollingTime": "00:00:10"
    },
    "MetricsCollectionSamplingInterval": "00:00:05",
    "MaximumLocalEnqueuedBackPressureThreshold": 10000,
    "BackPressurePollingInterval": "00:00:02",
    "PersistDeadLetterEnvelopes": true,
    "Listeners": [
      "tcp://localhost:2000",
      "tcp://localhost:2001"
    ],
    "Subscriptions": [
      {
        "Scope": "All",
        "Uri": "tcp://localhost:2002",
        "ContentTypes": [
          "application/json"
        ],
        "Match": null
      },
      {
        "Scope": "Type",
        "Uri": "tcp://localhost:2004",
        "ContentTypes": [
          "application/json"
        ],
        "Match": "Jasper.Testing.Message1"
      }
    ]
  }
}

</pre>

In the JSON section above, the primary sections you care about are:

* The `Listeners` section is an array that tells Jasper where and how to listen for incoming messages through registered transports
* The `Subscriptions` section allows you to make all necessary message subscriptions in configuration, but this is probably more easily
  accomplished by the <[linkto:documentation/messaging/routing;title=strong typed routing configuration]>.

## Programmatic Configuration


All system configuration for a Jasper application starts with the <[linkto:documentation/bootstrapping/configuring_jasper;title=JasperRegistry]> or `JasperOptionsBuilder` classes. Underneath `JasperRegistry` are these sections that are specific to the messaging support:

* `JasperRegistry.Handlers` - Configure policies about how message handlers are discovered, middleware is applied, and error handling policies. See <[linkto:documentation/messaging/handling]> for more information.
* `JasperRegistry.Publish` - Optionally declare what messages or events are published by the Jasper system and any static publishing rules. See <[linkto:documentation/messaging/routing]> for more information.
* `JasperRegistry.Transports` - Configure or disable the built in transports in Jasper. See <[linkto:documentation/messaging/transports]> for more information.

Sample usages of each of these sections are shown below:

<[sample:configuring-messaging-with-JasperRegistry]>

## Listen for Messages

You can direct Jasper to listen for incoming messages with the built in transports by just declaring
the incoming port for the transport or by providing a `Uri` that expresses both the transport type and
port number like this:

<[sample:MyListeningApp]>

Other transport types like the forthcoming RabbitMq and Azure Service Bus transports will probably be configured strictly
by using the `Uri` mechanism.


## Advanced Configuration

If desirable, you can directly manipulate the `JasperOptions` model from your system's `IConfiguration` with this bootstrapping model:

<[sample:using-configuration-with-jasperoptions]>

or its equivalent with `JasperRegistry`:

<[sample:ConfigUsingApp]>