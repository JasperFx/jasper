namespace Jasper.Testing.Http.ContentHandling
{
    public class HttpTestingApp : JasperRegistry
    {
        public HttpTestingApp()
        {
            Messages.Handlers.ConventionalDiscoveryDisabled = true;
        }
    }
}
