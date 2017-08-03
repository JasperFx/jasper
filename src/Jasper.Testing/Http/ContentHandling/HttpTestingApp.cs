namespace Jasper.Testing.Http.ContentHandling
{
    public class HttpTestingApp : JasperRegistry
    {
        public HttpTestingApp()
        {
            Messaging.Handlers.ConventionalDiscoveryDisabled = true;
        }
    }
}
