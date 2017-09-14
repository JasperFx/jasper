using Jasper.Bus.Transports.Configuration;

namespace Jasper.Testing.Samples
{
    // SAMPLE: IgnoreNoRoutes
    public class IgnoreNoRoutes : JasperRegistry
    {
        public IgnoreNoRoutes()
        {
            Advanced.NoMessageRouteBehavior = NoRouteBehavior.Ignore;
        }
    }
    // ENDSAMPLE
}
