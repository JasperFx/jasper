using System;
using System.Linq;
using Baseline;
using Jasper.Runtime;
using Jasper.Serialization;
using Jasper.Util;
using Shouldly;
using TestMessages;
using Xunit;

namespace Jasper.Testing.Runtime
{
    public class content_type_preferences_with_request_reply : IntegrationContext
    {
        public content_type_preferences_with_request_reply(DefaultApp @default) : base(@default)
        {
        }

        [Fact]
        public void envelope_has_accepts_for_known_response_readers()
        {
            var envelope = Publisher.As<MessageContext>().EnvelopeForRequestResponse<Message1>(new Message2());

            envelope.AcceptedContentTypes.ShouldContain("text/message1");
            envelope.AcceptedContentTypes.ShouldContain("text/oddball");

            envelope.AcceptedContentTypes.Last().ShouldBe("application/json");
        }
    }

    public class Message1TextReader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Message1).ToMessageTypeName();
        public Type DotNetType { get; } = typeof(Message1);
        public string ContentType { get; } = "text/message1";

        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

    }

    public class Message1OddballReader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Message1).ToMessageTypeName();
        public Type DotNetType { get; } = typeof(Message3);
        public string ContentType { get; } = "text/oddball";

        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

    }
}
