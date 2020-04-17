using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public class KafkaTransportProtocol<TKey, TVal> : ITransportProtocol<Message<TKey, TVal>>
    {
        private const string JasperMessageIdHeader = "Jasper_MessageId";
        public Message<TKey, TVal> WriteFromEnvelope(Envelope envelope)
        {
            var message = new Message<TKey, TVal>
            {
                Headers = new Headers(),
                Value = (TVal) envelope.Message
            };

            foreach (KeyValuePair<string, string> h in envelope.Headers)
            {
                Header header = new Header(h.Key, Encoding.UTF8.GetBytes(h.Value));
                message.Headers.Add(header);
            }

            message.Headers.Add(JasperMessageIdHeader, Encoding.UTF8.GetBytes(envelope.Id.ToString()));

            return message;
        }

        public Envelope ReadEnvelope(Message<TKey, TVal> message)
        {
            var env = new Envelope();

            foreach (var header in message.Headers.Where(h => !h.Key.StartsWith("Jasper")))
            {
                env.Headers.Add(header.Key, Encoding.UTF8.GetString(header.GetValueBytes()));
            }

            var messageIdHeader = message.Headers.Single(h => h.Key.Equals(JasperMessageIdHeader));
            env.Id = Guid.Parse(Encoding.UTF8.GetString(messageIdHeader.GetValueBytes()));
            env.Message = message.Value;

            return env;
        }
    }
}
