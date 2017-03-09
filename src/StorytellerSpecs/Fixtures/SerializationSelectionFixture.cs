using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Baseline;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Serializers;
using StoryTeller;
using StoryTeller.Engine;
using StoryTeller.Grammars.Tables;

namespace StorytellerSpecs.Fixtures
{
    public class StorytellerSpecsSystem : NulloSystem{}

    public class SerializerSelectionFixture : Fixture
    {
        public static void TryIt()
        {
            using (var runner = StorytellerRunner.For<StorytellerSpecsSystem>())
            {
                runner.Run("Serialization Selection / Serialization Selection Rules");
                runner.OpenResultsInBrowser();
            }
        }

        private IEnumerable<FakeSerializer> _serializers;
        private ChannelGraph _graph;
        private EnvelopeSerializer _envelopeSerializer;


        public void AvailableSerializers(string mimetypes)
        {
            var types = mimetypes.ToDelimitedArray(';');
            _serializers = types.Select(x => new FakeSerializer(x));
        }

        public void Preference(string mimetypes)
        {
            _graph = new ChannelGraph();
            _graph.AcceptedContentTypes.AddRange(mimetypes.ToDelimitedArray(';'));

            _envelopeSerializer = new EnvelopeSerializer(_graph, _serializers);
        }



        [ExposeAsTable("Outgoing Serialization Choice")]
        [FormatAs("{content}, {channel}, {envelope}, should be {selection}")]
        public string SerializationChoice(string content, string[] channel, string[] envelope)
        {
            var theEnvelope = new Envelope
            {
                ContentType = content,
                AcceptedContentTypes = envelope
            };

            var theNode = new ChannelNode("memory://1".ToUri());
            theNode.AcceptedContentTypes.AddRange(channel);

            var contentType = _envelopeSerializer
                .SelectSerializer(theEnvelope, theNode)
                ?.ContentType;

            WriteTrace("contentType is " + contentType);

            Console.WriteLine("I am running");

            throw new NotImplementedException(contentType);

            return contentType;
        }
    }

    public class FakeSerializer : IMessageSerializer
    {
        public FakeSerializer(string contentType)
        {
            ContentType = contentType;
        }

        public void Serialize(object message, Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public object Deserialize(Stream message)
        {
            throw new System.NotImplementedException();
        }

        public string ContentType { get; }
    }
}