using System.Text;
using Confluent.Kafka;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public class KafkaTransportProtocol<TKey> : ITransportProtocol<Message<TKey, byte[]>>
    {
        public Message<TKey, byte[]> WriteFromEnvelope(Envelope envelope)
        {
            var msg = new Message<TKey, byte[]>();

            msg.Value = envelope.Data;

            
            return msg;
        }

        public Envelope ReadEnvelope(Message<TKey, byte[]> message)
        {
            var env = new Envelope();

            foreach (var header in message.Headers)
            {
                env.Headers.Add(header.Key, Encoding.UTF8.GetString(header.GetValueBytes()));
            }

            env.Data = message.Value;

            return env;
        }
    }
}
