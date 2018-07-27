using Jasper;
using Jasper.Persistence.Marten;

namespace IntegrationTests.Persistence.Marten.Persistence.Sagas
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
