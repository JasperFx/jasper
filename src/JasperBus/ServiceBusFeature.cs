using System.Threading.Tasks;
using Jasper;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using JasperBus.Configuration;
using JasperBus.Model;
using JasperBus.Runtime;
using StructureMap;

namespace JasperBus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperBus.Generated");

        public ChannelGraph Channels { get; } = new ChannelGraph();

        public Policies Policies { get; } = new Policies();

        public void Dispose()
        {
            Channels.Dispose();
        }

        Task<Registry> IFeature.Bootstrap(JasperRegistry registry)
        {
            return bootstrap(registry);
        }

        Task IFeature.Activate(JasperRuntime runtime, IGenerationConfig generation)
        {
            return Task.Factory.StartNew(() =>
            {
                // TODO -- will need to be smart enough to do the conglomerate
                _graph.CompileAndBuildAll(generation, runtime.Container);

                var transports = runtime.Container.GetAllInstances<ITransport>();

                Channels.UseTransports(transports);

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

            _graph.Group();
            Policies.Apply(_graph);

            // TODO -- this will probably be a custom Registry later
            var services = new Registry();
            services.For<HandlerGraph>().Use(_graph);
            services.For<ChannelGraph>().Use(Channels);

            return services;
        }
    }
}