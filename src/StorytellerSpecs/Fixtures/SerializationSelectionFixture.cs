using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Bus.Configuration;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Serializers;
using Jasper.Conneg;
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
                runner.Run("SerializationExpression Selection / SerializationExpression Selection Rules");
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



        [ExposeAsTable("Outgoing SerializationExpression Choice")]
        [FormatAs("{content}, {channel}, {envelope}, should be {selection}")]
        public string SerializationChoice(string content, string[] channel, string[] envelope)
        {
            var theEnvelope = new Envelope
            {
                ContentType = content,
                AcceptedContentTypes = envelope.Where(x => x.IsNotEmpty()).ToArray()
            };

            var theNode = new ChannelNode("memory://1".ToUri());
            theNode.AcceptedContentTypes.AddRange(channel.Where(x => x.IsNotEmpty()));

            var contentType = _envelopeSerializer
                .SelectSerializer(theEnvelope, theNode)
                ?.ContentType;

            WriteTrace("contentType is " + contentType);

            return contentType;
        }
    }

    public class FakeSerializer : ISerializer, IMediaReader, IMediaWriter
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

        public string MessageType { get; }

        Type IMediaReader.DotNetType
        {
            get { throw new NotImplementedException(); }
        }

        Type IMediaWriter.DotNetType
        {
            get { throw new NotImplementedException(); }
        }

        public string ContentType { get; }
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task Write(object model, Stream stream)
        {
            throw new NotImplementedException();
        }

        public object Read(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> Read<T>(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IMediaReader[] ReadersFor(Type messageType)
        {
            throw new NotImplementedException();
        }

        public IMediaWriter[] WritersFor(Type messageType)
        {
            throw new NotImplementedException();
        }
    }
}
