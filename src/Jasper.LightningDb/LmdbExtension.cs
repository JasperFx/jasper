using Jasper;
using Jasper.Bus.Transports;
using Jasper.Configuration;
using Jasper.LightningDb;

[assembly:JasperModule(typeof(LmdbExtension))]

namespace Jasper.LightningDb
{
    public class LmdbExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Services.ForSingletonOf<IPersistence>().Use<LightningDbPersistence>();
        }
    }
}
