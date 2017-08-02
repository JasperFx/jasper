namespace Jasper.Testing.Http.ContentHandling
{
    public class HttpTestingApp : JasperRegistry
    {
        public HttpTestingApp()
        {
            Handlers.ConventionalDiscoveryDisabled = true;
        }
    }
}