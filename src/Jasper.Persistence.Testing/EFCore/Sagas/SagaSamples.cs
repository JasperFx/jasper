using Jasper.Persistence.Marten;

namespace Jasper.Persistence.Testing.EFCore.Sagas
{
    // SAMPLE: SagaApp-with-Marten
    public class MartenSagaApp : JasperOptions
    {
        public MartenSagaApp()
        {
            Extensions.Include<MartenBackedPersistence>();
        }
    }

    // ENDSAMPLE
}
