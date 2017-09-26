using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using BlueMilk.Scanning;
using Jasper.Bus;
using Jasper.Conneg;
using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus
{
    public class content_type_preferences_with_request_reply : IntegrationContext
    {
        [Fact]
        public void envelope_has_accepts_for_known_response_readers()
        {
            withAllDefaults();

            var envelope = Bus.As<ServiceBus>().EnvelopeForRequestResponse<Message1>(new Message2());

            envelope.AcceptedContentTypes.ShouldContain("text/message1");
            envelope.AcceptedContentTypes.ShouldContain("text/oddball");

            envelope.AcceptedContentTypes.Last().ShouldBe("application/json");
        }
    }

    public class Message1TextReader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Message1).ToMessageAlias();
        public Type DotNetType { get; } = typeof(Message1);
        public string ContentType { get; } = "text/message1";
        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotSupportedException();
        }
    }

    public class Message1OddballReader : IMessageDeserializer
    {
        public string MessageType { get; } = typeof(Message1).ToMessageAlias();
        public Type DotNetType { get; } = typeof(Message3);
        public string ContentType { get; } = "text/oddball";
        public object ReadFromData(byte[] data)
        {
            throw new NotSupportedException();
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotSupportedException();
        }
    }
}
