using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Runtime.Serializers;
using JasperBus.Transports.LightningQueues;
using StructureMap;

namespace JasperBus
{
    internal class ServiceBusRegistry : Registry
    {
        internal ServiceBusRegistry()
        {
            For<ITransport>().Singleton().Add<LightningQueuesTransport>();


            For<IEnvelopeSender>().Use<EnvelopeSender>();
            For<IServiceBus>().Use<ServiceBus>();
            For<IHandlerPipeline>().Use<HandlerPipeline>();

            ForSingletonOf<IEnvelopeSerializer>().Use<EnvelopeSerializer>();
            For<IMessageSerializer>().Add<JsonMessageSerializer>();

            ForSingletonOf<IReplyWatcher>().Use<ReplyWatcher>();
        }
    }
}