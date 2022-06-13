using System.Diagnostics;
using Baseline.Reflection;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Persistence.Marten.Persistence.Sagas;
using Jasper.Persistence.Sagas;

namespace Jasper.Persistence.Marten;

internal class MartenIntegration : IJasperExtension
{
    /// <summary>
    /// This directs the Marten integration to try to publish events out of the enrolled outbox
    /// for a Marten session on SaveChangesAsync()
    /// </summary>
    public bool ShouldPublishEvents { get; set; }

    public void Configure(JasperOptions options)
    {
        options.Advanced.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

        var frameProvider = new MartenSagaPersistenceFrameProvider();
        options.Advanced.CodeGeneration.SetSagaPersistence(frameProvider);
        options.Advanced.CodeGeneration.SetTransactions(frameProvider);

        options.Advanced.CodeGeneration.Sources.Add(new SessionVariableSource());

        options.Handlers.Discovery(x => x.IncludeTypes(type => type.HasAttribute<MartenCommandWorkflowAttribute>()));
    }
}
