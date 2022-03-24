<!--title:Topic Based Routing-->

While the <[linkto:documentation/integration/transports/azureservicebus]> and <[linkto:documentation/integration/transports/rabbitmq]> options both allow you to explicitly specify message routing rules to a specific named topic (*routing key* in Rabbit MQ parlance) through the fluent interface, there's another option that amounts to "publish these messages to a topic derived from the message type." This usage is valuable to allow you top opt into more sophisticated publish/subscribe routing utilizing all the power of tools like Rabbit MQ or Azure Service Bus or whatever other transports Jasper supports later.

Here's an example of configuring this option with Rabbit MQ:

snippet: sample_RabbitMqTopicSendingApp

And another example doing the same configuration with Azure Service Bus:

snippet: sample_AzureServiceBus_TopicSendingApp

Using one of the two configured applications above, you could send this message type:

snippet: sample_ItemCreatedWithTopic

like so:

snippet: sample_SendItemCreatedByTopic

In Jasper's internal routing, it would determine that the topic name for `ItemCreated` is *items* and publish to either the configured [Rabbit MQ topic exchange](https://www.rabbitmq.com/tutorials/tutorial-five-dotnet.html) or Azure Service Bus connection using the topic name *items*.


::: tip warning
Keep reading to the next section if you dislike using attributes, because the `[Topic]` attribute is not required on your message classes.
:::



## How Topic Name is Derived

When you publish a message with Jasper that is routed by topic name, the topic name is derived from the message and/or message type with this order of precedence:

1. A user-supplied topic name through the `IMessagePublisher.SendToTopic(message, topicName)` as shown in the section below titled *Explicitly Send to a Named Topic*
1. A `[Topic]` attribute directly on the message class
1. Any applicable *Topic Naming Rules* as shown in a section below
1. The <[linkto:documentation/execution/messages;title=message identifier]> for the message type, which can in turn be overridden with the `[MessageIdentity]` attribute


## Explicitly Send to a Named Topic

You can override the topic routing with explicit code like this sample shown below:

snippet: sample_SendItemCreatedToTopic


## Using the [Topic] Attribute

You can explicitly set the topic name for a message type by decorating it or its parent type with 
the `[Topic]` attribute like this:

snippet: sample_using_Topic_attribute


## Topic Naming Rules

You can use a *topic naming rule* in your system that derives the topic name for a message using some combination of the message type and message instance. 

Take for an example (stolen from Rabbit MQ documentation) where a custom logging message may be effectively routed by its `Priority`:

snippet: sample_LogMessageWithPriority

You can set up a topic naming rule to use the value of the `LogMessage.Priority` property as the topic name like so:

snippet: sample_AppWithTopicNamingRule

Finally, in usage you just use `IMessagePublisher.Send()` as you normally do:

snippet: sample_SendLogMessageToTopic
