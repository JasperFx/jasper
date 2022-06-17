# Jasper as Messaging Bus

There's certainly some value in Jasper just being a command bus running inside of a single process, but now
it's time to utilize Jasper to both publish and process messages received through external infrastructure like [Rabbit MQ](https://www.rabbitmq.com/)
or [Pulsar](https://pulsar.apache.org/).

## Terminology

To put this into perspective, here's how a Jasper application could be connected to the outside world:

![Jasper Messaging Architecture](/JasperMessaging.drawio.png)

:::tip
The diagram above should just say "Message Handler" as Jasper makes no differentiation between commands or events, but Jeremy is being too lazy to fix the diagram.
:::

Before going into any kind of detail about how to use Jasper messaging, let's talk about some terminology:

* *Transport* -- This refers to the support within Jasper for external messaging infrastructure tools like Rabbit MQ or Pulsar
* *Endpoint* -- A Jasper connection to some sort of external resource like a Rabbit MQ exchange or a Pulsar or Kafka topic. The [Async API](https://www.asyncapi.com/) specification refers to this as a *channel*, and Jasper may very well change its nomenclature in the future to be consistent with Async API
* *Sending Agent* -- You won't use this directly in your own code, but Jasper's internal adapters to publish outgoing messages to transport endpoints
* *Listener* -- Again, an internal detail of Jasper that receives messages from external transport endpoints, and mediates between the transports and executing the message handlers
* *Message Store* -- Database storage for Jasper's [inbox/outbox persistent messaging](/guide/persistence/)
* *Durability Agent* -- An internal subsystem in Jasper that runs in a background service to interact with the message store for Jasper's [transactional inbox/outbox](https://microservices.io/patterns/data/transactional-outbox.html) functionality

## Ping/Pong Sample

