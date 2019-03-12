using System;
using Baseline;
using Jasper;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Model;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Stub;
using Jasper.TestSupport.Storyteller.Logging;
using Lamar;
using Microsoft.Extensions.DependencyInjection;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    [Hidden]
    public class ServiceBusApplication : BusFixture
    {
        private JasperRegistry _registry;

        public override void SetUp()
        {
            _registry = new JasperRegistry();

            _registry.Services.AddSingleton(new MessageTracker());
            _registry.Services.AddSingleton(new MessageHistory());


            _registry.Services.For<IMessageLogger>().Use<StorytellerMessageLogger>().Singleton();
        }

        public override void TearDown()
        {
            var runtime = JasperHost.For(_registry);

            // Goofy, but gets things hooked up here
            runtime.Get<IMessageLogger>().As<StorytellerMessageLogger>().Start(Context);

            var history = runtime.Get<MessageHistory>();
            var graph = runtime.Get<HandlerGraph>();


            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);

            _registry.Publish.Message(type).To(channel);

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
