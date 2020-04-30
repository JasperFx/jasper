using Jasper.Configuration;
using Jasper.ConfluentKafka;

namespace Jasper.Kafka
{
    public class KafkaListenerConfiguration : ListenerConfiguration<KafkaListenerConfiguration, KafkaEndpoint>
    {
        public KafkaListenerConfiguration(KafkaEndpoint endpoint) : base(endpoint)
        {
        }
    }
}
