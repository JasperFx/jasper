# Messaging Transports

In Jasper parlance, a "transport" refers to one of Jasper's adapter libraries that enable the usage of an
external messaging infrastructure technology like Rabbit MQ or Pulsar. The local queues and [lightweight TCP transport](/tcp)
come in the box with Jasper, but you'll need an add on Nuget to enable any of the other transports.

## Key Abstractions

| Abstraction  | Description                                                                                                                                                                                                                                                 |
|--------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `ITransport` | Manages the connection to the messaging infrastructure like a Rabbit MQ broker and creates all the other objects referenced below                                                                                                                           |
| `Endpoint`   | The configuration for a sending or receiving address to your transport identified by a unique Uri scheme. For example, a Rabbit MQ endpoint may refer to a queue or an exchange and binding key. A TCP endpoint will refer to a server name and port number |
| `IListener`  | A service that helps read messages from the underlying message transport and relays those to Jasper as Jasper's `Envelope` structure                                                                                                                        |
| `ISender`    | A service that helps put Jasper `Envelope` structures out into the outgoing messaging infrastructure                                                                                                                                                        |

To build a new transport, we recommend looking first at the [Jasper.Pulsar](https://github.com/JasperFx/jasper/tree/master/src/Jasper.Pulsar) library
for a sample. At a bare minimum, you'll need to implement the services above, and also add some kind of `JasperOptions.Use[TransportName]()` extension
method to configure the connectivity to the messaging infrastructure and add the new transport to your Jasper application.

Also note, you will definitely want to use the [SendingCompliance](https://github.com/JasperFx/jasper/blob/master/src/TestingSupport/Compliance/SendingCompliance.cs)
tests in Jasper to verify that your new transport meets all Jasper requirements.
