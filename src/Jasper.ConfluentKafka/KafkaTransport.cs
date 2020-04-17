using System;
using System.Collections.Generic;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public static class Protocols
    {
        public const string Kafka = "kafka";

    }
    public class KafkaTransport<TKey, TVal> : TransportBase<KafkaEndpoint<TKey, TVal>>
    {
        public KafkaTransport(string protocol) : base(protocol)
        {
        }

        protected override IEnumerable<KafkaEndpoint<TKey, TVal>> endpoints()
        {
            throw new NotImplementedException();
        }

        protected override KafkaEndpoint<TKey, TVal> findEndpointByUri(Uri uri)
        {
            throw new NotImplementedException();
        }
    }
}
