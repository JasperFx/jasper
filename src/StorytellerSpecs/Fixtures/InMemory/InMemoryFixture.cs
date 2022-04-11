using System;
using System.Linq;
using Jasper;
using Jasper.Runtime.Handlers;
using Microsoft.Extensions.DependencyInjection;
using StoryTeller;

namespace StorytellerSpecs.Fixtures.InMemory
{
    public abstract class InMemoryFixture : Fixture
    {
        public static Uri Channel1 = new Uri("local://one");
        public static Uri Channel2 = new Uri("local://two");
        public static Uri Channel3 = new Uri("local://three");
        public static Uri Channel4 = new Uri("local://four");

        protected readonly Type[] messageTypes =
        {
            typeof(Message1), typeof(Message2), typeof(Message3), typeof(Message4), typeof(Message5), typeof(Message6)
        };


        protected InMemoryFixture()
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
    public class InMemoryServiceBusApplication : InMemoryFixture
    {
        private JasperOptions _options;

        public override void SetUp()
        {
            _options = new JasperOptions();
            _options.Services.AddSingleton(new MessageTracker());
        }

        public override void TearDown()
        {
            var runtime = JasperHost.For(_options);

            var graph = runtime.Get<HandlerGraph>();

            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType,
            [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);

            _options.Publish(x => x.Message(type).To(channel));

            // Just makes the test harness listen for things
            _options.ListenForMessagesFrom(channel);
        }
    }
}
