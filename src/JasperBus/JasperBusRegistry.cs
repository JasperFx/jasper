using Jasper;

namespace JasperBus
{
    public class JasperBusRegistry : JasperRegistry
    {
        public JasperBusRegistry()
        {
            UseFeature<ServiceBusFeature>();
        }

        public HandlerSource Handlers => Feature<ServiceBusFeature>().Handlers;
    }
}