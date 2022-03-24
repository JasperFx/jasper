<!--title:Apache Kafka Transport-->

::: tip warning
Jasper uses [Confluent Kakfa](https://github.com/confluentinc/confluent-kafka-dotnet) to communicate with Pulsar.
:::

## Installing

To use [Apache Kafka](https://kafka.apache.org/) as a transport with Jasper, first install the `Jasper.ConfluentKafka` library via nuget to your project. Behind the scenes, this package uses the [Confluent Kakfa](https://github.com/confluentinc/confluent-kafka-dotnet) to both send and receive messages from Pulsar.

snippet: sample_KafkaBasicExample
