using Jasper.Configuration;

namespace Jasper.ConfluentKafka
{
    public class KafkaSubscriberConfiguration : SubscriberConfiguration<KafkaSubscriberConfiguration, KafkaEndpoint>
    {
        public KafkaSubscriberConfiguration(KafkaEndpoint endpoint) : base(endpoint)
        {
        }

    }
}
