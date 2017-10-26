using Jasper;

namespace benchmarks
{
    public class Sender1 : JasperRegistry
    {
        public Sender1()
        {
            Services.Scan(_ =>
            {
                _.TheCallingAssembly();
                _.WithDefaultConventions();
            });

            Transports.Lightweight.ListenOnPort(2222);

            Publish.Message<Ping>().To("tcp://localhost:2233");
        }
    }
}