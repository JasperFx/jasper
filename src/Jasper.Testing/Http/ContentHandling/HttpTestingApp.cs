using Jasper.Testing.Bus.Compilation;
using Microsoft.Extensions.DependencyInjection;

namespace Jasper.Testing.Http.ContentHandling
{
    public class HttpTestingApp : JasperRegistry
    {
        public HttpTestingApp()
        {
            Handlers.DisableConventionalDiscovery(true);

            Services.AddTransient<IFakeStore, FakeStore>();
            Services.For<IWidget>().Use<Widget>();
            Services.For<IFakeService>().Use<FakeService>();
        }
    }
}
