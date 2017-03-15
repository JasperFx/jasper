using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Baseline;
using Baseline.Reflection;
using Jasper;
using JasperBus;
using JasperBus.Configuration;
using JasperBus.Model;
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

            _registry.Services.AddService<ITransport, StubTransport>();
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
            _registry.SendMessages(type.Name, t => t == type).To(channel);

            // Just makes the test harness listen for things
            _registry.ListenForMessagesFrom(channel);
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
        public void Handle(T message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Records.Add(new MessageRecord(envelope.ReceivedAt, message));
        }
    }

    public class Message1Handler : MessageHandler<Message1> { }
    public class Message2Handler : MessageHandler<Message2> { }
    public class Message3Handler : MessageHandler<Message3> { }
    public class Message4Handler : MessageHandler<Message4> { }
    public class Message5Handler : MessageHandler<Message5> { }

    public class Message6Handler : MessageHandler<Message6>
    {
    }

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


    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels = new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri));

        public StubTransport(string scheme = "stub")
        {
            ReplyChannel = Channels[new Uri($"{scheme}://replies")];
            Protocol = scheme;
        }

        public IEnumerable<StubMessageCallback> CallbackHistory()
        {
            return Channels.SelectMany(x => x.Callbacks);
        }

        public StubMessageCallback LastCallback()
        {
            return CallbackHistory().Last();
        }

        public StubChannel ReplyChannel { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public string Protocol { get; }
        public Uri ReplyUriFor(Uri node)
        {
            return ReplyChannel.Address;
        }

        public void Send(Uri uri, byte[] data, Dictionary<string, string> headers)
        {
            Channels[uri].Send(data, headers);
        }

        public Uri ActualUriFor(ChannelNode node)
        {
            return (node.Uri.AbsoluteUri + "/actual").ToUri();
        }

        public void ReceiveAt(ChannelNode node, IReceiver receiver)
        {
            Channels[node.Uri].StartReceiving(receiver);
        }

        public Uri CorrectedAddressFor(Uri address)
        {
            return address;
        }
    }

    public class StubChannel
    {
        public void Dispose()
        {
            WasDisposed = true;
        }

        public bool WasDisposed { get; set; }

        public StubChannel(Uri address)
        {
            Address = address;
        }

        public Uri Address { get; }

        public void StartReceiving(IReceiver receiver)
        {
            Receiver = receiver;
        }

        public IReceiver Receiver { get; set; }

        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public void Send(byte[] data, Dictionary<string, string> headers)
        {
            var callback = new StubMessageCallback(this);
            Callbacks.Add(callback);

            Receiver?.Receive(data, headers, callback).Wait();
        }
    }

    public class StubMessageCallback : IMessageCallback
    {
        private readonly StubChannel _channel;
        public readonly IList<Envelope> Sent = new List<Envelope>();
        public readonly IList<ErrorReport> Errors = new List<ErrorReport>();
        
        public StubMessageCallback(StubChannel channel)
        {
            _channel = channel;
        }

        public void MarkSuccessful()
        {
            MarkedSucessful = true;
        }

        public bool MarkedSucessful { get; set; }

        public void MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
        }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public void MoveToDelayedUntil(DateTime time)
        {
            DelayedTo = time;
        }

        public DateTime? DelayedTo { get; set; }

        public void MoveToErrors(ErrorReport report)
        {
            WasMovedToErrors = true;
            Errors.Add(report);
        }

        public bool WasMovedToErrors { get; set; }

        public void Requeue(Envelope envelope)
        {
            Requeued = true;
            _channel.Send(envelope.Data, envelope.Headers);
        }

        public bool Requeued { get; set; }

        public void Send(Envelope envelope)
        {
            Sent.Add(envelope);
        }

        public bool SupportsSend { get; } = true;
    }
}