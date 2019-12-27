using System;
using System.Threading.Tasks;
using Jasper.Serialization;
using Jasper.Util;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    internal class StubReader : IMessageDeserializer
    {
        public StubReader(Type messageType, string contentType)
        {
            MessageType = messageType.ToMessageTypeName();
            DotNetType = messageType;
            ContentType = contentType;
        }

        public string MessageType { get; }
        public Type DotNetType { get; }
        public string ContentType { get; }

        public object ReadFromData(byte[] data)
        {
            throw new NotImplementedException();
        }

    }
}
