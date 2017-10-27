using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Bus;
using Jasper.Conneg;
using Microsoft.AspNetCore.Http;

namespace StorytellerSpecs.Fixtures
{
    internal class FakeSerializerFactory : ISerializerFactory, IMessageDeserializer, IMessageSerializer
    {
        public FakeSerializerFactory(string contentType)
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

        Type IMessageDeserializer.DotNetType
        {
            get { throw new NotImplementedException(); }
        }

        Type IMessageSerializer.DotNetType
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

        public IMessageDeserializer[] ReadersFor(Type messageType, MediaSelectionMode mode)
        {
            return new IMessageDeserializer[0];
        }

        public IMessageSerializer[] WritersFor(Type messageType, MediaSelectionMode mode)
        {
            return new IMessageSerializer[]{new FakeWriter(messageType, ContentType), };
        }

        public IMessageDeserializer VersionedReaderFor(Type incomingType)
        {
            throw new NotImplementedException();
        }
    }
}
