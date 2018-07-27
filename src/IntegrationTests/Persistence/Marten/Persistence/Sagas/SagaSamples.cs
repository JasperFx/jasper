using Jasper.Persistence.Marten;

namespace Jasper.Marten.Tests.Persistence.Sagas
{
    // SAMPLE: SagaApp-with-Marten
    public class MartenSagaApp : JasperRegistry
    {
        public MartenSagaApp()
        {
            Include<MartenBackedPersistence>();
        }
    }

    // ENDSAMPLE
}
