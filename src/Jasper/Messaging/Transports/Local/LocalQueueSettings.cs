using System;
using System.Collections.Generic;
using Jasper.Configuration;
using Jasper.Util;

namespace Jasper.Messaging.Transports.Local
{
    public class LocalQueueSettings : Endpoint
    {
        public LocalQueueSettings(string name)
        {
            Name = name.ToLowerInvariant();
            Uri = $"local://{name}".ToUri();
        }

        public LocalQueueSettings(Uri uri) : base(uri)
        {

        }

        public override void Parse(Uri uri)
        {
            Name = LocalTransport.QueueName(uri);
            IsDurable = uri.IsDurable();
        }


        public IList<Subscription> Subscriptions { get; } = new List<Subscription>();

        protected internal override void StartListening(IMessagingRoot root, ITransportRuntime runtime)
        {
            throw new NotImplementedException();
        }
    }
}
