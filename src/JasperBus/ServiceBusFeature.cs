using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Codegen;
using Jasper.Configuration;
using JasperBus.Configuration;
using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Runtime.Serializers;
using StructureMap;
using Policies = JasperBus.Configuration.Policies;

namespace JasperBus
{
    public class ServiceBusFeature : IFeature
    {
        private HandlerGraph _graph;
        public HandlerSource Handlers { get; } = new HandlerSource();

        public GenerationConfig Generation { get; } = new GenerationConfig("JasperBus.Generated");

        public ChannelGraph Channels { get; } = new ChannelGraph();

        public Policies Policies { get; } = new Policies();

        public readonly Registry Services = new ServiceBusRegistry();

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
                // generation config of the base, with service bus specific stuff
                _graph.Compile(generation, runtime.Container);

                var transports = runtime.Container.GetAllInstances<ITransport>().ToArray();

                Channels.UseTransports(transports);

                configureSerializationOrder(runtime);

                // TODO
                // Done -- 1. Start up transports
                // 2. Start up subscriptions when ready

                var pipeline = runtime.Container.GetInstance<IHandlerPipeline>();

                // TODO -- might parallelize this later
                foreach (var transport in transports)
                {
                    transport.Start(pipeline, Channels);

                    Channels
                        .Where(x => x.Uri.Scheme == transport.Protocol && x.Sender == null)
                        .Each(x =>
                        {
                            x.Sender = new NulloSender(transport, x.Uri);
                        });
                }
            });


        }

        private void configureSerializationOrder(JasperRuntime runtime)
        {
            var contentTypes = runtime.Container.GetAllInstances<IMessageSerializer>().Select(x => x.ContentType).ToArray();

            var unknown = Channels.AcceptedContentTypes.Where(x => !contentTypes.Contains(x)).ToArray();
            if (unknown.Any())
            {
                throw new UnknownContentTypeException(unknown, contentTypes);
            }

            foreach (var contentType in contentTypes)
            {
                Channels.AcceptedContentTypes.Fill(contentType);
            }


        }

        private async Task<Registry> bootstrap(JasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);

            _graph.Group();
            Policies.Apply(_graph);

            Services.For<HandlerGraph>().Use(_graph);
            Services.For<ChannelGraph>().Use(Channels);

            if (registry.Logging.UseConsoleLogging)
            {
                Services.For<IBusLogger>().Add<ConsoleBusLogger>();
            }

            return Services;
        }
    }
}