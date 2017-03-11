using System.Threading.Tasks;
using Jasper;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using JasperBus.Model;
using StructureMap;

namespace JasperBus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public void Dispose()
        {
            // shut down transports
        }

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime, GenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                // TODO -- will need to be smart enough to do the conglomerate
                _graph.CompileAndBuildAll(generation, runtime.Container);

                // TODO
                // 1. Start up transports
                // 2. Start up subscriptions when ready

            });


        }

        private async Task<Registry> bootstrap(IJasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            // TODO -- this will have to merge in config from the service bus feature!!!
            _graph = new HandlerGraph();
            _graph.AddRange(calls);

            

            // TODO -- this will probably be a custom Registry later
            var services = new Registry();
            services.For<HandlerGraph>().Use(_graph);

            return services;
        }
    }
}