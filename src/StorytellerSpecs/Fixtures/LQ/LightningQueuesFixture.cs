using System;
using System.Linq;
using Jasper;
using Jasper.Messaging.Model;
using Microsoft.Extensions.DependencyInjection;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.LQ
{
    public abstract class LightningQueuesFixture : Fixture
    {
        public static Uri Channel1 = new Uri("durable://localhost:2201/one");
        public static Uri Channel2 = new Uri("durable://localhost:2201/two");
        public static Uri Channel3 = new Uri("durable://localhost:2201/three");
        public static Uri Channel4 = new Uri("durable://localhost:2201/four");

        protected readonly Type[] messageTypes =
        {
            typeof(Message1), typeof(Message2), typeof(Message3), typeof(Message4), typeof(Message5), typeof(Message6)
        };


        protected LightningQueuesFixture()
        {
            AddSelectionValues("MessageTypes", messageTypes.Select(x => x.Name).ToArray());
            AddSelectionValues("Channels", Channel1.ToString(), Channel2.ToString(), Channel3.ToString(),
                Channel4.ToString());
        }

        protected Type messageTypeFor(string name)
        {
            return messageTypes.First(x => x.Name == name);
        }
    }

    [Hidden]
    public class LQServiceBusApplication : LightningQueuesFixture
    {
        private JasperRegistry _registry;


        public override void SetUp()
        {
            _registry = new JasperRegistry();
            _registry.Services.AddSingleton(new MessageTracker());
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);

            var graph = runtime.Get<HandlerGraph>();

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
    }
}
