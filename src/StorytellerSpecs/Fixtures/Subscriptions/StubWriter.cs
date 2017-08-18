using System;
using System.Threading.Tasks;
using Jasper.Conneg;
using Microsoft.AspNetCore.Http;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    public class StubWriter : IMediaWriter
    {
        public StubWriter(Type messageType, string contentType)
        {
            DotNetType = messageType;
            ContentType = contentType;
        }

        public Type DotNetType { get; }
        public string ContentType { get; }
        public byte[] Write(object model)
        {
            throw new NotImplementedException();
        }

        public Task WriteToStream(object model, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }
}