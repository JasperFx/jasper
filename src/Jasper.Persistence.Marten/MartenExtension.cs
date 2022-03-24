using Jasper;
using Jasper.Configuration;
using Jasper.Persistence.Durability;
using Jasper.Persistence.Marten;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Persistence.Marten.Persistence.Sagas;
using Jasper.Persistence.Postgresql;
using Jasper.Persistence.Sagas;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Persistence.Marten
{
    #region sample_MartenExtension
    public class MartenExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Services.AddTransient<IEnvelopePersistence, PostgresqlEnvelopePersistence>();
            options.Services.AddSingleton(Options);

            options.Advanced.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

            var frameProvider = new MartenSagaPersistenceFrameProvider();
            options.Advanced.CodeGeneration.SetSagaPersistence(frameProvider);
            options.Advanced.CodeGeneration.SetTransactions(frameProvider);

            options.Services.AddSingleton<IDocumentStore>(x =>
            {
                var documentStore = new DocumentStore(Options);
                return documentStore;
            });

            options.Handlers.GlobalPolicy<FineGrainedSessionCreationPolicy>();


            options.Services.AddScoped(c => c.GetService<IDocumentStore>().OpenSession());
            options.Services.AddScoped(c => c.GetService<IDocumentStore>().QuerySession());

            options.Advanced.CodeGeneration.Sources.Add(new SessionVariableSource());

            options.Services.AddSingleton(s =>
            {
                var store = s.GetRequiredService<IDocumentStore>();

                return new PostgresqlSettings
                {
                    // Super hacky, look away!!!
                    ConnectionString = store.Storage.Database.CreateConnection().ConnectionString,
                    SchemaName = store.Options.DatabaseSchemaName
                };
            });

        }

        public StoreOptions Options { get; } = new StoreOptions();
    }
    #endregion
}

