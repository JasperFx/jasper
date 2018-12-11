using Jasper;
using Jasper.Configuration;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Codegen;
using Marten;
using Microsoft.Extensions.DependencyInjection;

// SAMPLE: MartenExtension
[assembly: JasperModule(typeof(MartenExtension))]

namespace Jasper.Persistence.Marten
{
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperOptionsBuilder registry)
        {
            registry.Services.AddSingleton<IDocumentStore>(x =>
            {
                var storeOptions = x.GetService<StoreOptions>();
                var documentStore = new DocumentStore(storeOptions);
                return documentStore;
            });

            registry.Handlers.GlobalPolicy<FineGrainedSessionCreationPolicy>();


            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().OpenSession());
            registry.Services.AddScoped(c => c.GetService<IDocumentStore>().QuerySession());

            registry.CodeGeneration.Sources.Add(new SessionVariableSource());
        }
    }
}
// ENDSAMPLE
