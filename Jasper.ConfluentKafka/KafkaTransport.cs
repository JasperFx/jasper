using System;
using System.Collections.Generic;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public class KafkaTransport<TKey, TVal> : TransportBase<KafkaEndpoint<TKey, TVal>>
    {
        public static readonly string ProtocolName = "kafka";

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
