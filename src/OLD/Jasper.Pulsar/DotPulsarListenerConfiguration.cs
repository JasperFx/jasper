using Jasper.Configuration;

namespace Jasper.DotPulsar
{
    public class DotPulsarListenerConfiguration : ListenerConfiguration<DotPulsarListenerConfiguration, DotPulsarEndpoint>
    {
        public DotPulsarListenerConfiguration(DotPulsarEndpoint endpoint) : base(endpoint)
        {
        }
    }
}
