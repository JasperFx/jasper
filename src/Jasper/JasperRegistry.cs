using Jasper.Configuration;

namespace Jasper
{
    public class JasperRegistry
    {
        public ServiceRegistry Services { get; } = new ServiceRegistry();
    }
}