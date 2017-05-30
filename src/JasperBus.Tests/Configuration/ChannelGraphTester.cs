using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Serializers;
using JasperBus.Tests.Stubs;
using NSubstitute;
using Shouldly;
using Xunit;

namespace JasperBus.Tests.Configuration
{
    public class when_building_an_envelope_for_sending_that_does_not_match_an_existing_channel
    {
        public readonly StubTransport _transport = new StubTransport();
        public readonly Envelope _envelope = new Envelope();
        private readonly ChannelGraph _graph;
        public readonly IEnvelopeSerializer _serializer = Substitute.For<IEnvelopeSerializer>();
        private readonly Envelope _sentEnvelope;

        public when_building_an_envelope_for_sending_that_does_not_match_an_existing_channel()
        {
            _graph = new ChannelGraph(_transport, new StubTransport("fake"), new StubTransport("other"));
            _graph.AcceptedContentTypes.Add("text/xml");
            _graph.AcceptedContentTypes.Add("text/json");

            _sentEnvelope = _graph.Send(_envelope, "stub://one".ToUri(), _serializer).Result;
        }

        [Fact]
        public void not_the_same_envelope()
        {
            // this is actually important for things to work correctly
            _sentEnvelope.ShouldNotBeSameAs(_envelope);
        }

        [Fact]
        public void should_call_the_serializer()
        {
            _serializer.Received().Serialize(_sentEnvelope, null);
        }

        [Fact]
        public void should_tag_the_accepted_content_types_from_the_graph()
        {
            _sentEnvelope.AcceptedContentTypes.ShouldHaveTheSameElementsAs("text/xml", "text/json");
        }

        [Fact]
        public void should_have_the_corrected_uri_address()
        {
            _sentEnvelope.Destination.ShouldBe(_transport.CorrectedAddressFor("stub://one".ToUri()));
        }

        [Fact]
        public void should_have_the_reply_uri()
        {
            _sentEnvelope.ReplyUri.ShouldBe(_transport.ReplyChannel.Address);
        }
    }

    public class when_building_an_envelope_for_sending_that_matches_an_existing_channel
    {
        public readonly StubTransport theTransport = new StubTransport();
        public readonly Envelope theEnvelope = new Envelope();
        private readonly ChannelGraph theGraph;
        public readonly IEnvelopeSerializer theSerializer = Substitute.For<IEnvelopeSerializer>();
        private Envelope theSentEnvelope;
        private ChannelNode theNode;

        public when_building_an_envelope_for_sending_that_matches_an_existing_channel()
        {
            theGraph = new ChannelGraph(theTransport, new StubTransport("fake"), new StubTransport("other"));
            theGraph.AcceptedContentTypes.Add("text/xml");
            theGraph.AcceptedContentTypes.Add("text/json");



            var address = "stub://one".ToUri();



            theNode = theGraph[address];
            theNode.Sender = new NulloSender(theTransport, theNode.Uri);
            theNode.ShouldNotBeNull();
            theNode.Destination = "remote://one".ToUri();
            theNode.ReplyUri = "stub://replies".ToUri();

            theSentEnvelope = theGraph.Send(theEnvelope, address, theSerializer).Result;


        }

        [Fact]
        public void not_the_same_envelope()
        {
            // this is actually important for things to work correctly
            theSentEnvelope.ShouldNotBeSameAs(theEnvelope);
        }

        [Fact]
        public void should_call_the_serializer()
        {
            theSerializer.Received().Serialize(theSentEnvelope, theNode);
        }

        [Fact]
        public void should_tag_the_accepted_content_types_from_the_graph()
        {
            theSentEnvelope.AcceptedContentTypes.ShouldHaveTheSameElementsAs("text/xml", "text/json");
        }

        [Fact]
        public void should_have_the_corrected_uri_address()
        {
            theSentEnvelope.Destination.ShouldBe(theNode.Destination);
        }

        [Fact]
        public void should_have_the_reply_uri()
        {
            theSentEnvelope.ReplyUri.ShouldBe(theNode.ReplyUri);
        }
    }
}
