using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Conneg;

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
            return new IMediaWriter[]{new FakeWriter(messageType, ContentType), };
        }
    }
}