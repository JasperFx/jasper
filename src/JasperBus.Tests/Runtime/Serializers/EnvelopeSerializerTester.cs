using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Serializers;
using Newtonsoft.Json;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Runtime.Serializers
{
    public class EnvelopeSerializerTester
    {
        private IMessageSerializer[] messageSerializers;
        private EnvelopeSerializer theSerializer;
        private Address theAddress;
        private ChannelGraph theGraph;

        public EnvelopeSerializerTester()
        {
            messageSerializers = new IMessageSerializer[]
                {new JsonMessageSerializer(new JsonSerializerSettings())};
            theGraph = new ChannelGraph();
            theSerializer = new EnvelopeSerializer(theGraph, messageSerializers);

            theAddress = new Address { City = "Jasper", State = "Missouri" };
        }


        private void assertRoundTrips(int index)
        {
            var contentType = messageSerializers[index].ContentType;
            var envelope = new Envelope()
            {
                Message = theAddress,
                ContentType = contentType
            };

            var node = new ChannelNode(new Uri("stub://1"));
            theSerializer.Serialize(envelope, node);

            envelope.Message = theSerializer.Deserialize(envelope, node);

            envelope.Message.ShouldNotBeTheSameAs(theAddress);
            envelope.Message.ShouldBe(theAddress);
        }

        [Fact]
        public void can_round_trip_with_each_serializer_type()
        {
            assertRoundTrips(0);
        }

        [Fact]
        public void happily_chooses_the_default_content_type_for_the_graph_if_none_is_on_the_envelope()
        {
            var envelope = new Envelope()
            {
                Message = theAddress,
                ContentType = null
            };

            var node = new ChannelNode(new Uri("stub://1"));
            node.AcceptedContentTypes.Add("application/json");

            theSerializer.Serialize(envelope, node);

            envelope.ContentType.ShouldBe("application/json");

            envelope.Message = theSerializer.Deserialize(envelope, node);

            envelope.Message.ShouldNotBeTheSameAs(theAddress);
            envelope.Message.ShouldBe(theAddress);

        }
    }


    public class EnvelopeSerializer_sad_path_Tester : InteractionContext<EnvelopeSerializer>
    {
        private IMessageSerializer[] serializers;
        private Envelope theEnvelope;
        private ChannelNode theNode = new ChannelNode(new Uri("stub://1"));

        public EnvelopeSerializer_sad_path_Tester()
        {
            serializers = Services.CreateMockArrayFor<IMessageSerializer>(5);
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

            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(theEnvelope, theNode);
            }).Message.ShouldContain("random/nonexistent");
        }

        [Fact]
        public void throws_on_serialize_with_no_message()
        {
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
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
                var messageSerializer = new JsonMessageSerializer(new JsonSerializerSettings());
                var serializer = new EnvelopeSerializer(null, new[] { messageSerializer });
                var envelope = new Envelope(Encoding.UTF8.GetBytes("garbage"), new Dictionary<string, string>(), null);
                envelope.ContentType = messageSerializer.ContentType;
                serializer.Deserialize(envelope, theNode);
            }).Message.ShouldBe("Message serializer has failed");
        }

        [Fact]
        public void ask_for_a_content_type_that_does_not_exist()
        {
            theEnvelope.ContentType = "weird";
            Exception<InvalidOperationException>.ShouldBeThrownBy(() => {
                ClassUnderTest.Serialize(theEnvelope, theNode);
            });
        }


    }
}