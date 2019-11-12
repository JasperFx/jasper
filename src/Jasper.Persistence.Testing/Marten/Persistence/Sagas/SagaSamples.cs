using Jasper.Persistence.Marten;

namespace Jasper.Persistence.Testing.Marten.Persistence.Sagas
{
    // SAMPLE: SagaApp-with-Marten
    public class MartenSagaApp : JasperOptions
    {
        public MartenSagaApp()
        {
            Include<MartenBackedPersistence>();
        }
    }

    // ENDSAMPLE
}
