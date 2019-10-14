using System;
using System.IO;
using System.Threading.Tasks;
using Jasper.Conneg;

namespace StorytellerSpecs.Fixtures
{
    internal class FakeSerializerFactory : ISerializerFactory<IMessageDeserializer, IMessageSerializer>, IMessageDeserializer, IMessageSerializer
    {
        public FakeSerializerFactory(string contentType)
        {
            ContentType = contentType;
        }

        public string MessageType { get; }

        Type IReaderStrategy.DotNetType => throw new NotImplementedException();

        public object ReadFromData(byte[] data)
        {
            throw new NotImplementedException();
        }

        Type IWriterStrategy.DotNetType => throw new NotImplementedException();

        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public object Deserialize(Stream message)
        {
            throw new NotImplementedException();
        }

        public string ContentType { get; }

        public IMessageDeserializer ReaderFor(Type messageType)
        {
            return null;
        }

        public IMessageSerializer WriterFor(Type messageType)
        {
            return new FakeWriter(messageType, ContentType);
        }

        public void Serialize(object message, Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}
