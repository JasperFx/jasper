using Jasper;
using Jasper.Configuration;
using JasperBus.Marten;
using JasperBus.Marten.Codegen;
using Marten;

[assembly:JasperModule(typeof(MartenExtension))]

namespace JasperBus.Marten
{
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<StoreOptions>();

            registry.Services.ForSingletonOf<IDocumentStore>().Use(x => new DocumentStore(x.GetInstance<StoreOptions>()));

            registry.Services.For<IDocumentSession>().Use("Default DocumentSession", c => c.GetInstance<IDocumentStore>().OpenSession());
            registry.Services.For<IQuerySession>().Use("Default QuerySession", c => c.GetInstance<IDocumentStore>().QuerySession());


            registry.Generation.Sources.Add(new SessionVariableSource());
        }
    }
}
