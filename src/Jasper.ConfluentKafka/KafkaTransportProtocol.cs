using System.Collections.Generic;
using System.Linq;
using System.Text;
using Confluent.Kafka;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public class KafkaTransportProtocol : ITransportProtocol<Message<byte[], byte[]>>
    {
        public const string KafkaMessageKeyHeader = "Confluent.Kafka.Message.Key";

        public Message<byte[], byte[]> WriteFromEnvelope(Envelope envelope)
        {
            var message = new Message<byte[], byte[]>
            {
                Headers = new Headers(),
                Value = envelope.Data
            };

            IDictionary<string, object> envelopHeaders = new Dictionary<string, object>();
            envelope.WriteToDictionary(envelopHeaders);

            var headers = new Headers();
            foreach (Header header in envelopHeaders.Select(h => new Header(h.Key, Encoding.UTF8.GetBytes(h.Value.ToString()))))
            {
                headers.Add(header);
            }

            message.Headers = headers;

            if (!envelopHeaders.TryGetValue(KafkaMessageKeyHeader, out object msgKey)) return message;

            if (msgKey is byte[] key)
            {
                message.Key = key;
            }
            else
            {
                message.Key = Encoding.UTF8.GetBytes(msgKey.ToString());
            }

            return message;
        }

        public Envelope ReadEnvelope(Message<byte[], byte[]> message)
        {
            var env = new Envelope()
            {
                Data = message.Value
            };

            Dictionary<string, object> incomingHeaders = message.Headers.Select(h => new {h.Key, Value = h.GetValueBytes()})
                .ToDictionary(k => k.Key, v => (object)Encoding.UTF8.GetString(v.Value));

            if(message.Key != null && !incomingHeaders.ContainsKey(KafkaMessageKeyHeader))
            {
                env.Headers.Add(KafkaMessageKeyHeader, Encoding.UTF8.GetString(message.Key));
            }
            
            env.ReadPropertiesFromDictionary(incomingHeaders);

            return env;
        }
    }
}
