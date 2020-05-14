using Jasper.Configuration;
using Jasper.Pulsar;

namespace Jasper.Pulsar
{
    public class PulsarListenerConfiguration : ListenerConfiguration<PulsarListenerConfiguration, PulsarEndpoint>
    {
        public PulsarListenerConfiguration(PulsarEndpoint endpoint) : base(endpoint)
        {
        }
    }
}
