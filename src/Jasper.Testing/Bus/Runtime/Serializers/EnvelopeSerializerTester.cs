using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Runtime.Serializers
{


    public class EnvelopeSerializer_sad_path_Tester : InteractionContext<EnvelopeSerializer>
    {
        private ISerializer[] serializers;
        private Envelope theEnvelope;
        private ChannelNode theNode = new ChannelNode(new Uri("stub://1"));

        public EnvelopeSerializer_sad_path_Tester()
        {
            serializers = Services.CreateMockArrayFor<ISerializer>(5);
            for (int i = 0; i < serializers.Length; i++)
            {
                serializers[i].ContentType.Returns("text/" + i);
            }

            theEnvelope = new Envelope()
            {
                Data = new byte[0]
            };
        }


        [Fact]
        public void chooses_by_mimetype()
        {
            theEnvelope.ContentType = serializers[3].ContentType;
            var o = new object();
            theEnvelope.Data = new byte[100];

            serializers[3].Deserialize(Arg.Any<Stream>()).Returns(o);

            ClassUnderTest.Deserialize(theEnvelope, theNode).ShouldBeTheSameAs(o);
        }

        [Fact]
        public void throws_on_unknown_content_type()
        {
            theEnvelope.ContentType = "random/nonexistent";
            theEnvelope.Message = new object();

            ShouldBeStringTestExtensions.ShouldContain(Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(theEnvelope, theNode);
            }).Message, "random/nonexistent");
        }

        [Fact]
        public void throws_on_serialize_with_no_message()
        {
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(new Envelope(), theNode);
            });
        }

        [Fact]
        public void throws_on_deserialize_with_no_data()
        {
            Exception<EnvelopeDeserializationException>.ShouldBeThrownBy(() =>
            {
                ClassUnderTest.Deserialize(new Envelope(), theNode);
            });
        }

        [Fact]
        public void throws_on_deserialize_of_bad_message()
        {
            Exception<EnvelopeDeserializationException>.ShouldBeThrownBy(() =>
            {
                var messageSerializer = new NewtonsoftSerializer(new BusSettings());
                var serializer = new EnvelopeSerializer(null, new HandlerGraph(), new[] { messageSerializer }, new List<IMediaReader>(), new List<IMediaWriter>());
                var envelope = new Envelope(Encoding.UTF8.GetBytes("garbage"), new Dictionary<string, string>(), null);
                envelope.ContentType = messageSerializer.ContentType;
                serializer.Deserialize(envelope, theNode);
            });
        }

        [Fact]
        public void ask_for_a_content_type_that_does_not_exist()
        {
            theEnvelope.ContentType = "weird";
            Exception<ArgumentOutOfRangeException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(theEnvelope, theNode);
            });
        }


    }
}
