<!--title:Reading, Writing, and Versioning Messages, Commands, and Events-->

Jasper ultimately needs to be able to dehydrate any published message to a `byte[]` then ship that information along with the related header
metadata to the receiving application that will ultimately hydrate that `byte[]` back to a .Net object. Out of the box, Jasper comes with support for using [Newtonsoft.Json](https://www.newtonsoft.com/json) to serialize and deserialize objects for
transport across the wire. The easiest, and likely starting point, for using Jasper for messaging is to use a shared DTO (Data Transfer Object) library that
exposes the shared message types that can easily be serialized and deserialized from and to Json.

This is probably going to be fine in many circumstances, but you could easily need to:

* Use a more efficient or at least different serialization mechanism
* Avoid having to share DTO types between services to prevent the coupling that causes
* Support the concept of versioned messages so that your system can evolve without breaking other systems that either subscribe to or publish
  to your system
* Use some kind of custom reader or writer code against your message objects that doesn't necessarily use a serializer of some sort

Fortunately, Jasper has some mechanisms explained in this topic to address the use cases above.

<div class="alert alert-info"><b>Note!</b> The way that Jasper chooses how to read and write message data is largely influenced by the concept
of <a href="https://en.wikipedia.org/wiki/Content_negotiation">content negotiation</a> from HTTP</div>


First though, it might help to understand how Jasper reads the message when it receives a new `Envelope`:

1. It first looks at the `message-type` header in the incoming `Envelope`
1. Using that value, it finds all the available `IMessageSerializer` strategies that match that message type, and
   tries to select one that matches the value of the `content-type` header
1. Invoke the matching reader to read the raw `byte[]` data into a .Net object
1. Now that you know the actual .Net type for the message, select the proper message handler and off it goes

## Message Type Identity

Let's say that you have a basic message structure like this:

snippet: sample_PersonBorn1

By default, Jasper will identify this type by just using the .Net full name like so:

snippet: sample_ootb_message_alias

However, if you want to explicitly control the message type because you aren't sharing the DTO types or for some
other reason (readability? diagnostics?), you can override the message type alias with an attribute:

snippet: sample_override_message_alias

Which now gives you different behavior:

snippet: sample_explicit_message_alias


## Versioning

By default, Jasper will just assume that any message is "V1" unless marked otherwise.
Going back to the original `PersonBorn` message class in previous sections, let's say that you
create a new version of that message that is no longer structurally equivalent to the original message:

snippet: sample_PersonBorn_V2

The `[Version("V2")]` attribute usage tells Jasper that this class is "V2" for the `message-type` = "person-born."

Jasper will now accept or publish this message using the built in Json serialization with the content type of `application/vnd.person-born.v2+json`.
Any custom serializers should follow some kind of naming convention for content types that identify versioned representations.

## Message Serializers and Deserializers

You can create custom message deserializers for a message by providing your own implementation of the `IMessageDeserializer` interface from Jasper:

snippet: sample_IMediaReader

The easiest way to do this is to just subclass the base `MessageDeserializerBase<T>` class as shown below:

snippet: sample_BlueTextReader

Likewise, to provide a custom message serializer for a message type, you need to implement the `IMessageSerializer` interface shown below:

snippet: sample_IMediaWriter

Again, the easiest way to implement this interface is to subclass the `MessageSerializerBase<T>` class as shown below:

snippet: sample_GreenTextWriter

`IMessageDeserializer` and `IMessageSerializer` classes in the main application assembly are automatically discovered and applied by Jasper. If you need to add custom
reader or writers from another assembly, you just need to add them to the underlying IoC container like so:

snippet: sample_RegisteringCustomReadersAndWriters


## Custom Serializers

To use additional .Net serializers, you just need to create a new implementation of the `Jasper.Conneg.ISerializerFactory` interface and register
that into the IoC service container.

snippet: sample_ISerializer

See the [built in Newtonsoft.Json adapter](https://github.com/JasperFx/jasper/blob/master/src/Jasper/Conneg/Json/NewtonsoftSerializerFactory.cs) for an example usage.


## Versioned Message Forwarding

If you make breaking changes to an incoming message in a later version, you can simply handle both versions of that message separately:

snippet: sample_PersonCreatedHandler

Or you could use a custom `IMessageDeserializer` to read incoming messages from V1 into the new V2 message type, or you can take advantage of message forwarding
so you only need to handle one message type using the `IForwardsTo<T>` interface as shown below:

snippet: sample_IForwardsTo<PersonBornV2>

Which forwards to the current message type:

snippet: sample_PersonBorn_V2

Using this strategy, other systems could still send your system the original `application/vnd.person-born.v1+json` formatted
message, and on the receiving end, Jasper would know to deserialize the Json data into the `PersonBorn` object, then call its
`Transform()` method to build out the `PersonBornV2` type that matches up with your message handler.


## Customizing Json Serialization

Just in case the default Json serialization isn't quite what you need, you can customize the Json serialization inside
of your `JasperRegistry` class like so:

snippet: sample_CustomizingJsonSerialization




