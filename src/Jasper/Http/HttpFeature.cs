using System.Threading.Tasks;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperHttp.Model;
using JasperHttp.Routing;
using StructureMap;

namespace JasperHttp
{
    public class HttpServices : ServiceRegistry
    {
        public HttpServices(RouteGraph routes)
        {
            For<RouteGraph>().Use(routes);
            For<IUrlRegistry>().Use(routes.Router.Urls);
        }
    }

    public class HttpFeature : IFeature
    {
        public readonly ActionSource Actions = new ActionSource();

        public readonly RouteGraph Routes = new RouteGraph();

        public HttpFeature()
        {
            Actions.IncludeClassesSuffixedWithEndpoint();
        }

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperHttp.Generated");

        public void Dispose()
        {
            // TODO -- not sure what this would need to do. Kestrel would get
            // shut down from the Container being disposed
        }

        public Task<Registry> Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        public Task Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                Routes.BuildRoutingTree(generation, runtime.Container);
            });
        }

        private async Task<Registry> bootstrap(JasperRegistry registry)
        {
            var actions = await Actions.FindActions(registry.ApplicationAssembly);
            foreach (var methodCall in actions)
            {
                Routes.AddRoute(methodCall);
            }

            return new HttpServices(Routes);
        }
    }
}