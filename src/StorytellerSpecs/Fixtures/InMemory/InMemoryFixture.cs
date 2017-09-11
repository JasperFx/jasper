using System;
using System.Linq;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Model;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.InMemory
{
    public abstract class InMemoryFixture : Fixture
    {
        public static Uri Channel1 = new Uri("loopback://one");
        public static Uri Channel2 = new Uri("loopback://two");
        public static Uri Channel3 = new Uri("loopback://three");
        public static Uri Channel4 = new Uri("loopback://four");

        protected readonly Type[] messageTypes = new Type[] { typeof(Message1), typeof(Message2), typeof(Message3), typeof(Message4), typeof(Message5), typeof(Message6) };


        protected InMemoryFixture()
        {
            AddSelectionValues("MessageTypes", messageTypes.Select(x => x.Name).ToArray());
            AddSelectionValues("Channels", Channel1.ToString(), Channel2.ToString(), Channel3.ToString(), Channel4.ToString());
        }

        protected Type messageTypeFor(string name)
        {
            return messageTypes.First(x => x.Name == name);
        }
    }

    [Hidden]
    public class InMemoryServiceBusApplication : InMemoryFixture
    {
        private JasperRegistry _registry;

        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _registry.Services.ForConcreteType<MessageTracker>().Configure.Singleton();
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);

            var graph = runtime.Container.GetInstance<HandlerGraph>();

            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.Publish.MessagesMatching(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.Transports.ListenForMessagesFrom(channel);
        }
    }
}
