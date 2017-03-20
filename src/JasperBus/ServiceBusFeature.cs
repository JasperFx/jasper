using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper;
using Jasper.Codegen;
using Jasper.Codegen.StructureMap;
using Jasper.Configuration;
using JasperBus.Configuration;
using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Runtime.Serializers;
using JasperBus.Transports.LightningQueues;
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
                // generation config of the base, with service bus specific stuff
                _graph.Compile(generation, runtime.Container);

                var transports = runtime.Container.GetAllInstances<ITransport>().ToArray();

                Channels.UseTransports(transports);

                var serializers = runtime.Container.GetAllInstances<IMessageSerializer>();
                Channels.AcceptedContentTypes.AddRange(serializers.Select(x => x.ContentType));

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

        private async Task<Registry> bootstrap(IJasperRegistry registry)
        {
            var calls = await Handlers.FindCalls(registry).ConfigureAwait(false);

            _graph = new HandlerGraph();
            _graph.AddRange(calls);

            _graph.Group();
            Policies.Apply(_graph);


            // TODO -- this will probably be a custom Registry later
            var services = new Registry();
            services.For<HandlerGraph>().Use(_graph);
            services.For<ChannelGraph>().Use(Channels);
            services.For<ITransport>().Singleton().Add<LightningQueuesTransport>();


            services.For<IEnvelopeSender>().Use<EnvelopeSender>();
            services.For<IServiceBus>().Use<ServiceBus>();
            services.For<IHandlerPipeline>().Use<HandlerPipeline>();

            services.ForSingletonOf<IEnvelopeSerializer>().Use<EnvelopeSerializer>();
            services.For<IMessageSerializer>().Use<JsonMessageSerializer>();

            return services;
        }
    }
}