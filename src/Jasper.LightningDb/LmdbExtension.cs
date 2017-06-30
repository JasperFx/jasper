using Jasper;
using Jasper.Bus.Runtime;
using Jasper.Configuration;
using Jasper.LightningDb;
using Jasper.LightningDb.Transport;

[assembly:JasperModule(typeof(LmdbExtension))]

namespace Jasper.LightningDb
{
    public class LmdbExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.For<ITransport>().Use<PersistentTransport>().Singleton();
        }
    }
}
