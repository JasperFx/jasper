using System;
using System.Linq;
using Baseline.Dates;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Logging;
using Jasper.Bus.Model;
using Jasper.Bus.Runtime;
using Jasper.Bus.Tracking;
using Jasper.Bus.Transports;
using Jasper.LightningDb;
using Jasper.Storyteller.Logging;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    [Hidden]
    public class ServiceBusApplication : BusFixture
    {
        private JasperRegistry _registry;
        private bool _waitForSubscriptions;

        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _waitForSubscriptions = false;

            _registry.Services.AddTransient<ITransport, StubTransport>();
            _registry.Services.ForConcreteType<MessageTracker>().Configure.Singleton();

            _registry.Services.ForConcreteType<MessageHistory>().Configure.Singleton();
            _registry.Services.AddTransient<IBusLogger, MessageTrackingLogger>();

            _registry.Services.For<LightningDbSettings>().Use(new LightningDbSettings
            {
                MaxDatabases = 20
            });

            _registry.Logging.LogBusEventsWith(new StorytellerBusLogger(Context));
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);
            var history = runtime.Container.GetInstance<MessageHistory>();
            var graph = runtime.Container.GetInstance<HandlerGraph>();


            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.Publish.MessagesMatching(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.Transports.ListenForMessagesFrom(channel);
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

        [FormatAs("Listen for incoming messages from {channel}")]
        public void ListenForMessagesFrom([SelectionList("Channels")] Uri channel)
        {
            _registry.Transports.ListenForMessagesFrom(channel);
        }

    }
}
