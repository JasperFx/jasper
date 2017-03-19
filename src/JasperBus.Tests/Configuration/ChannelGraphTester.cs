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
        public readonly StubTransport theTransport = new StubTransport();
        public readonly Envelope theEnvelope = new Envelope();
        private readonly ChannelGraph theGraph;
        public readonly IEnvelopeSerializer theSerializer = Substitute.For<IEnvelopeSerializer>();
        private Envelope theSentEnvelope;

        public when_building_an_envelope_for_sending_that_does_not_match_an_existing_channel()
        {
            theGraph = new ChannelGraph(theTransport, new StubTransport("fake"), new StubTransport("other"));
            theGraph.AcceptedContentTypes.Add("text/xml");
            theGraph.AcceptedContentTypes.Add("text/json");

            theSentEnvelope = theGraph.Send(theEnvelope, "stub://one".ToUri(), theSerializer);
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
            theSerializer.Received().Serialize(theSentEnvelope, null);
        }

        [Fact]
        public void should_tag_the_accepted_content_types_from_the_graph()
        {
            theSentEnvelope.AcceptedContentTypes.ShouldHaveTheSameElementsAs("text/xml", "text/json");
        }

        [Fact]
        public void should_have_the_corrected_uri_address()
        {
            theSentEnvelope.Destination.ShouldBe(theTransport.CorrectedAddressFor("stub://one".ToUri()));
        }

        [Fact]
        public void should_have_the_reply_uri()
        {
            theSentEnvelope.ReplyUri.ShouldBe(theTransport.ReplyChannel.Address);
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

            theSentEnvelope = theGraph.Send(theEnvelope, address, theSerializer);


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