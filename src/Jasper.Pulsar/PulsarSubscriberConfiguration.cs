using Jasper.Configuration;

namespace Jasper.Pulsar
{
    public class PulsarSubscriberConfiguration : SubscriberConfiguration<PulsarSubscriberConfiguration, PulsarEndpoint>
    {
        public PulsarSubscriberConfiguration(PulsarEndpoint endpoint) : base(endpoint)
        {
        }

    }
}
