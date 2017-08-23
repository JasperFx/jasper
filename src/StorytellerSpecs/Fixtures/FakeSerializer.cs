using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Conneg;
using Microsoft.AspNetCore.Http;

namespace StorytellerSpecs.Fixtures
{
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

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotImplementedException();
        }

        public object ReadFromData(byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotImplementedException();
        }

        public IMediaReader[] ReadersFor(Type messageType)
        {
            return new IMediaReader[0];
        }

        public IMediaWriter[] WritersFor(Type messageType)
        {
            return new IMediaWriter[]{new FakeWriter(messageType, ContentType), };
        }

        public IMediaReader VersionedReaderFor(Type incomingType)
        {
            throw new NotImplementedException();
        }
    }
}
