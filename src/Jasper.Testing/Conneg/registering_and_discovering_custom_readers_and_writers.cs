using System;
using Jasper.Conneg;
using Jasper.Conneg.Json;
using Jasper.Messaging.Runtime.Serializers;
using Jasper.Util;
using Shouldly;
using TestingSupport;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Conneg
{
    public class registering_and_discovering_custom_readers_and_writers : IntegrationContext
    {
        public registering_and_discovering_custom_readers_and_writers(DefaultApp @default) : base(@default)
        {
        }

        public MessagingSerializationGraph theSerialization => Host.Get<MessagingSerializationGraph>();

        public class ConnegHandler
        {
            public void Handle(Message4 message)
            {
            }
        }

        [Fact]
        public void can_override_json_serialization_for_a_mesage()
        {
            // Not overridden, so it should be the default
            theSerialization.WriterFor(typeof(Message1))["application/json"]
                .ShouldBeOfType<NewtonsoftJsonWriter>();

            // Overridden
            theSerialization.WriterFor(typeof(OverriddenJsonMessage))["application/json"]
                .ShouldBeOfType<OverrideJsonWriter>();
        }

        [Fact]
        public void can_override_json_serialization_reader_for_a_message_type()
        {
            // Not overridden, so it should be the default
            theSerialization.ReaderFor(typeof(Message4).ToMessageTypeName())["application/json"]
                .ShouldBeOfType<NewtonsoftJsonReader>();

            // Overridden
            theSerialization.ReaderFor(typeof(OverriddenJsonMessage).ToMessageTypeName())["application/json"]
                .ShouldBeOfType<OverrideJsonReader>();
        }

        [Fact]
        public void scans_for_custom_readers_in_the_app_assembly()
        {
            theSerialization.ReaderFor(typeof(Message1).ToMessageTypeName())
                .ContentTypes.ShouldContain("green");
        }

        [Fact]
        public void scans_for_custom_writers_in_the_app_assembly()
        {
            theSerialization.WriterFor(typeof(Message5)).ContentTypes
                .ShouldHaveTheSameElementsAs("application/json", "green", "blue");
        }
    }

    public class Message1Handler
    {
        public void Handle(Message1 message)
        {
        }
    }

    public class OverriddenJsonMessage
    {
    }

    public class OverrideJsonWriter : IMessageSerializer
    {
        public Type DotNetType { get; } = typeof(OverriddenJsonMessage);
        public string ContentType { get; } = "application/json";

        public byte[] Write(object model)
        {
            throw new NotSupportedException();
        }

    }

    public class OverrideJsonReader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(OverriddenJsonMessage).ToMessageTypeName();
        public Type DotNetType { get; } = typeof(OverriddenJsonMessage);
        public string ContentType { get; } = "application/json";

        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

    }

    public class GreenMessage1Reader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Message1).ToMessageTypeName();
        public Type DotNetType { get; } = typeof(Message1);
        public string ContentType { get; } = "green";

        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

    }


    public class GreenMessage1Writer : IMessageSerializer
    {
        public Type DotNetType { get; } = typeof(Message5);
        public string ContentType { get; } = "green";

        public byte[] Write(object model)
        {
            throw new NotSupportedException();
        }

    }

    public class BlueMessage1Writer : IMessageSerializer
    {
        public Type DotNetType { get; } = typeof(Message5);
        public string ContentType { get; } = "blue";

        public byte[] Write(object model)
        {
            throw new NotSupportedException();
        }

    }
}
