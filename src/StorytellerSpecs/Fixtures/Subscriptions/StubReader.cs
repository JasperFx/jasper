using System;
using System.Threading.Tasks;
using Jasper.Conneg;
using Jasper.Util;
using Microsoft.AspNetCore.Http;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    internal class StubReader : IMessageDeserializer
    {
        public StubReader(Type messageType, string contentType)
        {
            MessageType = messageType.ToMessageAlias();
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

        public Task<T> ReadFromRequest<T>(HttpRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
