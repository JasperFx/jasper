﻿using System;
using System.Threading.Tasks;
using Jasper.Conneg;

namespace StorytellerSpecs.Fixtures.Subscriptions
{
    internal class StubWriter : IMessageSerializer
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

    }
}
