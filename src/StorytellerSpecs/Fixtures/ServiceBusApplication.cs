using System;
using Jasper;
using JasperBus;
using JasperBus.Model;
using JasperBus.Runtime;
using JasperBus.Tracking;
using JasperBus.Transports.LightningQueues;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    [Hidden]
    public class ServiceBusApplication : BusFixture
    {
        private JasperBusRegistry _registry;


        public override void SetUp()
        {
            _registry = new JasperBusRegistry();

            _registry.Services.AddService<ITransport, StubTransport>();
            _registry.Services.ForConcreteType<MessageTracker>().Configure.Singleton();

            _registry.Services.ForConcreteType<MessageHistory>().Configure.Singleton();
            _registry.Services.AddService<IBusLogger, MessageTrackingLogger>();

            _registry.Services.For<LightningQueueSettings>().Use(new LightningQueueSettings
            {
                MaxDatabases = 20
            });
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);

            var graph = runtime.Container.GetInstance<HandlerGraph>();

            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.SendMessages(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.ListenForMessagesFrom(channel);
        }

        [FormatAs("When a Message1 is received, it cascades a matching Message2")]
        public void ReceivingMessage1CascadesMessage2()
        {
            _registry.Handlers.IncludeType<Cascader1>();
        }

        [FormatAs("When Message2 is received, it cascades matching Message3 and Message4")]
        public void ReceivingMessage2CascadesMultiples()
        {
            _registry.Handlers.IncludeType<Cascader2>();
        }
    }
}