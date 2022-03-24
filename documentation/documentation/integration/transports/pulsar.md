<!--title:Apache Pulsar Transport-->

::: tip warning
Jasper uses [DotPulsar](https://github.com/apache/pulsar-dotpulsar) to communicate with Pulsar.
:::

## Installing

To use [Apache Pulsar](https://pulsar.apache.org/) as a transport with Jasper, first install the `Jasper.DotPulsar` library via nuget to your project. Behind the scenes, this package uses the [DotPulsar](https://github.com/apache/pulsar-dotpulsar) to both send and receive messages from Pulsar.

snippet: sample_PulsarSendReceiveExample
