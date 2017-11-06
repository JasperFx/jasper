using Jasper;
using Jasper.Bus.Transports;
using Jasper.Configuration;
using Jasper.Marten;
using Jasper.Marten.Codegen;
using Marten;
using Microsoft.Extensions.DependencyInjection;

// SAMPLE: MartenExtension
[assembly:JasperModule(typeof(MartenExtension))]

namespace Jasper.Marten
{
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Settings.Require<StoreOptions>();

            registry.Services.AddSingleton<IDocumentStore>(x =>
            {
                var storeOptions = x.GetService<StoreOptions>();
                return new DocumentStore(storeOptions);
            });

            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().OpenSession());
            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().QuerySession());


            registry.Generation.Sources.Add(new SessionVariableSource());


        }
    }
}
// ENDSAMPLE
