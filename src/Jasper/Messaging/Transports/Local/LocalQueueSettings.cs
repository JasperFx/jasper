using System;
using System.Collections.Generic;
using Jasper.Configuration;
using Jasper.Messaging.Transports.Sending;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Local
{
    public class LocalQueueSettings : Endpoint
    {
        public LocalQueueSettings(string name)
        {
            Name = name.ToLowerInvariant();
        }

        public LocalQueueSettings(Uri uri) : base(uri)
        {

        }

        public override Uri Uri => $"local://{Name}".ToUri();

        public override void Parse(Uri uri)
        {
            Name = LocalTransport.QueueName(uri);
            IsDurable = uri.IsDurable();
        }

        public override Uri ReplyUri()
        {
            return IsDurable ? $"local://durable/{Name}".ToUri() : $"local://{Name}".ToUri();
        }

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            throw new NotSupportedException();
        }

        protected override ISender CreateSender(IMessagingRoot root)
        {
            throw new NotSupportedException();
        }

        public override string ToString()
        {
            return $"Local Queue '{Name}'";
        }
    }
}
