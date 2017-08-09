using System.Text;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Jasper.Testing.Bus.Runtime;
using Newtonsoft.Json;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Serializers
{
    public class SerializationGraphTester
    {
        [Fact]
        public void can_try_to_deserialize_an_envelope_with_no_message_type()
        {
            var serialization = SerializationGraph.Basic();

            var json = JsonConvert.SerializeObject(new Message2(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var envelope = new Envelope()
            {
                Data = Encoding.UTF8.GetBytes(json),
                MessageType = null,
                ContentType = "application/json"
            };

            serialization.Deserialize(envelope, new ChannelNode("loopback://one".ToUri()))
                .ShouldBeOfType<Message2>();
        }

        [Fact]
        public void can_try_to_deserialize_an_envelope_with_no_message_type_or_content_type()
        {
            var serialization = SerializationGraph.Basic();

            var json = JsonConvert.SerializeObject(new Message2(), new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            var envelope = new Envelope()
            {
                Data = Encoding.UTF8.GetBytes(json),
                MessageType = null,
                ContentType = null
            };

            serialization.Deserialize(envelope, new ChannelNode("loopback://one".ToUri()))
                .ShouldBeOfType<Message2>();
        }
    }
}
