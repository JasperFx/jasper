using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper;
using JasperBus;
using JasperBus.Runtime;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{

    public abstract class BusFixture : Fixture
    {
        public static Uri Channel1 = new Uri("stub://one");
        public static Uri Channel2 = new Uri("stub://two");
        public static Uri Channel3 = new Uri("stub://three");
        public static Uri Channel4 = new Uri("stub://four");

        protected readonly Type[] messageTypes = new Type[] {typeof(Message1), typeof(Message2), typeof(Message3), typeof(Message4), typeof(Message5), typeof(Message6)};


        protected BusFixture()
        {
            AddSelectionValues("MessageTypes", messageTypes.Select(x => x.Name).ToArray());
            AddSelectionValues("Channels", "stub://one", "stub://two", "stub://three", "stub://four");
        }

        protected Type messageTypeFor(string name)
        {
            return messageTypes.First(x => x.Name == name);
        }


    }

    [Hidden]
    public class ServiceBusApplication : BusFixture
    {
        private JasperBusRegistry _registry;



        public override void SetUp()
        {
            _registry = new JasperBusRegistry();

            _registry.Services.ForConcreteType<MessageTracker>().Configure.Singleton();
        }

        public override void TearDown()
        {
            var runtime = JasperRuntime.For(_registry);
            Context.State.Store(runtime);
        }

        [FormatAs("Sends message {messageType} to {channel}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, [SelectionList("Channels")] Uri channel)
        {
            var type = messageTypeFor(messageType);
            _registry.SendMessages(type.Name, t => t == type);
        }
    }

    public class SendMessageFixture : BusFixture
    {


        private JasperRuntime _runtime;

        public SendMessageFixture()
        {
            Title = "Send Messages through the Service Bus";
        }


        public IGrammar IfTheApplicationIs()
        {
            return Embed<ServiceBusApplication>("If a service bus application is configured to")
                .After(c => _runtime = c.State.Retrieve<JasperRuntime>());
        }

        [FormatAs("Send message {messageType} named {name}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            _runtime.Container.GetInstance<IServiceBus>().Send(message);
        }

        [FormatAs("Send message {messageType} named {name} directly to {address}")]
        public void SendMessageDirectly([SelectionList("MessageTypes")] string messageType, string name, [SelectionList("Channels")] Uri address)
        {
            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            _runtime.Container.GetInstance<IServiceBus>().Send(address, message);
        }

        public IGrammar TheMessagesSentShouldBe()
        {
            return VerifySetOf(sent).Titled("All the messages sent should be")
                .MatchOn(x => x.ReceivedAt, x => x.MessageType, x => x.Name);
        }

        private IList<MessageRecord> sent()
        {
            return _runtime.Container.GetInstance<MessageTracker>().Records;
        }

        public override void TearDown()
        {
            _runtime.Dispose();
        }
    }

    public abstract class Message
    {
        public string Name { get; set; }
    }

    public class Message1 : Message { }
    public class Message2 : Message { }
    public class Message3 : Message { }
    public class Message4 : Message { }
    public class Message5 : Message { }
    public class Message6 : Message { }

    public abstract class MessageHandler<T> where T : Message
    {
        public static void Handle(T message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Records.Add(new MessageRecord(envelope.ReceivedAt, message));
        }
    }

    public class Message1Handler : MessageHandler<Message1> { }
    public class Message2Handler : MessageHandler<Message2> { }
    public class Message3Handler : MessageHandler<Message3> { }
    public class Message4Handler : MessageHandler<Message4> { }
    public class Message5Handler : MessageHandler<Message5> { }
    public class Message6Handler : MessageHandler<Message6> { }

    public class MessageTracker
    {
        public readonly IList<MessageRecord> Records = new List<MessageRecord>();

    }

    public class MessageRecord
    {
        public MessageRecord(Uri receivedAt, Message message)
        {
            MessageType = message.GetType().Name;
            Name = message.Name;
            ReceivedAt = receivedAt;
        }

        public Uri ReceivedAt { get; set; }

        public string Name { get; set; }

        public string MessageType { get; set; }
    }
}