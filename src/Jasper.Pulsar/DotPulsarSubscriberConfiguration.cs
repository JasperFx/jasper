using Jasper.Configuration;

namespace Jasper.DotPulsar
{
    public class DotPulsarSubscriberConfiguration : SubscriberConfiguration<DotPulsarSubscriberConfiguration, DotPulsarEndpoint>
    {
        public DotPulsarSubscriberConfiguration(DotPulsarEndpoint endpoint) : base(endpoint)
        {
        }

    }
}
