using System.Diagnostics;
using Jasper.Persistence.Marten.Codegen;
using Jasper.Persistence.Marten.Persistence.Sagas;
using Jasper.Persistence.Sagas;

namespace Jasper.Persistence.Marten;

internal class MartenIntegration : IJasperExtension
{
    public MartenIntegration()
    {
        Debug.WriteLine("Hi");
    }

    public bool ShouldPublishEvents { get; set; }

    public void Configure(JasperOptions options)
    {
        options.Advanced.CodeGeneration.Sources.Add(new MartenBackedPersistenceMarker());

        var frameProvider = new MartenSagaPersistenceFrameProvider();
        options.Advanced.CodeGeneration.SetSagaPersistence(frameProvider);
        options.Advanced.CodeGeneration.SetTransactions(frameProvider);

        options.Advanced.CodeGeneration.Sources.Add(new SessionVariableSource());
    }
}
