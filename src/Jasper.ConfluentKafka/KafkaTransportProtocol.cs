using System.Text;
using Confluent.Kafka;
using Jasper.Transports;

namespace Jasper.ConfluentKafka
{
    public class KafkaTransportProtocol<TKey, TVal> : ITransportProtocol<Message<TKey, TVal>>
    {
        public Message<TKey, TVal> WriteFromEnvelope(Envelope envelope) =>
            new Message<TKey, TVal>
            {
                Headers = new Headers(),
                Value = (TVal) envelope.Message
            };

        public Envelope ReadEnvelope(Message<TKey, TVal> message)
        {
            var env = new Envelope();

            foreach (var header in message.Headers)
            {
                env.Headers.Add(header.Key, Encoding.UTF8.GetString(header.GetValueBytes()));
            }

            env.Message = message.Value;

            return env;
        }
    }
}
