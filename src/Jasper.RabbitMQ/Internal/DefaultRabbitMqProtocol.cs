using System;
using System.Collections.Generic;
using System.Text;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    // SAMPLE: DefaultRabbitMqProtocol
    public class DefaultRabbitMqProtocol : Protocol<IBasicProperties>, IRabbitMqProtocol
    {
        public DefaultRabbitMqProtocol()
        {
            MapProperty(x => x.CorrelationId, (e, p) => e.CorrelationId = p.MessageId, (e,p) => p.MessageId = e.CorrelationId);
        }

        protected override void writeOutgoingHeader(IBasicProperties outgoing, string key, string value)
        {
            outgoing.Headers[key] = value;
        }

        protected override bool tryReadIncomingHeader(IBasicProperties incoming, string key, out string value)
        {
            if (incoming.Headers.TryGetValue(key, out var raw))
            {
                value = raw is byte[] b ? Encoding.Default.GetString(b) : raw.ToString();
                return true;
            }

            value = null;
            return false;
        }

        protected override void writeIncomingHeaders(IBasicProperties incoming, Envelope envelope)
        {
            foreach (var pair in incoming.Headers)
            {
                envelope.Headers[pair.Key] = pair.Value is byte[] b ? Encoding.Default.GetString(b) : pair.Value?.ToString();
            }
        }
    }
    // ENDSAMPLE
}
