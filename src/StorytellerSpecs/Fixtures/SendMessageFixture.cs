using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using JasperBus;
using JasperBus.Configuration;
using JasperBus.Runtime;
using JasperBus.Runtime.Invocation;
using JasperBus.Tracking;
using JasperBus.Transports.LightningQueues;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{


    public class SendMessageFixture : BusFixture
    {
        private JasperRuntime _runtime;
        private Task _task;

        public SendMessageFixture()
        {
            Title = "Send Messages through the Service Bus";
        }

        public override void SetUp()
        {
            LightningQueuesTransport.DeleteAllStorage();

        }

        public IGrammar IfTheApplicationIs()
        {
            return Embed<ServiceBusApplication>("If a service bus application is configured to")
                .After(c => _runtime = c.State.Retrieve<JasperRuntime>());
        }

        [FormatAs("Send message {messageType} named {name}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            var history = _runtime.Container.GetInstance<MessageHistory>();

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(() =>
            {
                _runtime.Container.GetInstance<IServiceBus>().Send(message).Wait();
            });

            waiter.Wait(5.Seconds());

            StoryTellerAssert.Fail(!waiter.IsCompleted, "Messages were never completely tracked");
        }

        [FormatAs("Send message {messageType} named {name} directly to {address}")]
        public void SendMessageDirectly([SelectionList("MessageTypes")] string messageType, string name,
            [SelectionList("Channels")] Uri address)
        {
            var history = _runtime.Container.GetInstance<MessageHistory>();

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(async () =>
            {
                await bus().Send(address, message).ConfigureAwait(false);
            });

            waiter.Wait(5.Seconds());
        }

        private IServiceBus bus()
        {
            return _runtime.Container.GetInstance<IServiceBus>();
        }

        public IGrammar TheMessagesSentShouldBe()
        {
            return VerifySetOf(sent).Titled("All the messages sent should be")
                .MatchOn(x => x.ReceivedAt, x => x.MessageType, x => x.Name);
        }

        [FormatAs("Request/Reply: send a Message1 with name {name} should respond with a matching reply")]
        public Task RequestAndReply(string name)
        {
            var message = new Message1 {Name = name};

            return bus().Request<Message2>(message).ContinueWith(t =>
            {
                _runtime.Container.GetInstance<MessageTracker>().Records.Add(new MessageRecord("stub://replies".ToUri(), t.Result));
            });
        }

        private IList<MessageRecord> sent()
        {
            return _runtime.Container.GetInstance<MessageTracker>().Records;
        }

        [FormatAs("Send a Message1 named 'Ack' that we expect to succeed and wait for the ack")]
        public void SendMessageSuccessfully()
        {
            _task = bus().SendAndWait(new Message1{Name = "Ack"});
        }

        [FormatAs("Send a message that will fail with an AmbiguousMatchException exception")]
        public void SendMessageUnsuccessfully()
        {
            _task = bus().SendAndWait(new ErrorMessage());
        }

        [FormatAs("The acknowledgement was received within 3 seconds")]
        public bool AckIsReceived()
        {
            try
            {
                _task.Wait(3.Seconds());
            }
            catch (Exception )
            {
                // swallow the ex for the sake of the test
            }
            return _task.IsCompleted || _task.IsFaulted;
        }

        [FormatAs("The acknowledgement was successful")]
        public bool AckWasSuccessful()
        {
            StoryTellerAssert.Fail(_task.IsFaulted || !_task.IsCompleted, () => _task.Exception?.ToString() ?? "Task was not completed");

            return true;
        }

        [FormatAs("The acknowledgment failed and contained the message {message}")]
        public bool TheAckFailedWithMessage(string message)
        {
            StoryTellerAssert.Fail(_task.Exception == null, "The task exception is null");

            StoryTellerAssert.Fail(!_task.Exception.InnerExceptions.First().ToString().Contains(message), "The actual exception text was:\n" + _task.Exception.ToString());

            return true;
        }

        [FormatAs("Send a message with an unknown content type to {address}")]
        public async Task SendMessageWithUnknownContentType([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = new Envelope(bytes, new Dictionary<string, string>(), null);
            envelope.ContentType = "text/xml";

            envelope.Destination = address;

            var sender = _runtime.Container.GetInstance<IEnvelopeSender>();
            await sender.Send(envelope);
        }

        [FormatAs("Send a garbled message to {address}")]
        public async Task SendGarbledMessage([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = new Envelope(bytes, new Dictionary<string, string>(), null);
            envelope.ContentType = "application/json";

            envelope.Destination = address;

            var sender = _runtime.Container.GetInstance<IEnvelopeSender>();
            await sender.Send(envelope);
        }

        public override void TearDown()
        {
            _runtime.Dispose();

            // Let LQ cooldown
            Thread.Sleep(1000);
        }
    }

    public class ErrorMessage
    {

    }

    public class ErrorMessageHandler
    {
        public void Handle(ErrorMessage message)
        {
            throw new AmbiguousMatchException();
        }
    }

    public abstract class Message
    {
        public string Name { get; set; }
    }


    public class UnhandledMessage : Message
    {
        
    }

    public class Message1 : Message
    {
    }

    public class Message2 : Message
    {
    }

    public class Message3 : Message
    {
    }

    public class Message4 : Message
    {
    }

    public class Message5 : Message
    {
    }

    public class Message6 : Message
    {
    }

    public class Cascader1
    {
        public Message2 Handle(Message1 message)
        {
            return new Message2{Name = message.Name};
        }
    }

    public class Cascader2
    {
        public object[] Handle(Message2 message)
        {
            return new object[]
            {
                new Message3{Name = message.Name},
                new Message4{Name = message.Name}
            };
        }
    }

    public abstract class MessageHandler<T> where T : Message
    {
        public void Handle(T message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Records.Add(new MessageRecord(envelope.ReceivedAt, message));
        }
    }

    public class Message1Handler : MessageHandler<Message1>
    {
    }

    public class Message2Handler : MessageHandler<Message2>
    {
    }

    public class Message3Handler : MessageHandler<Message3>
    {
    }

    public class Message4Handler : MessageHandler<Message4>
    {
    }

    public class Message5Handler : MessageHandler<Message5>
    {
    }

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
        public readonly LightweightCache<Uri, StubChannel> Channels =
            new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri));

        public StubTransport(string scheme = "stub")
        {
            ReplyChannel = Channels[new Uri($"{scheme}://replies")];
            Protocol = scheme;
        }

        public StubChannel ReplyChannel { get; set; }

        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public string Protocol { get; }

        public Task Send(Uri uri, byte[] data, IDictionary<string, string> headers)
        {
            return Channels[uri].Send(data, headers);
        }

        public void Start(IHandlerPipeline pipeline, ChannelGraph channels)
        {
            foreach (var node in channels.IncomingChannelsFor(Protocol))
            {
                var receiver = new Receiver(pipeline, channels, node);
                ReceiveAt(node, receiver);
            }

            var replyNode = new ChannelNode(ReplyChannel.Address);
            var replyReceiver = new Receiver(pipeline, channels, replyNode);
            ReceiveAt(replyNode, replyReceiver);

            channels.Where(x => x.Uri.Scheme == Protocol).Each(x => {
                x.ReplyUri = ReplyChannel.Address;
                x.Destination = x.Uri;
            });
        }

        public Uri DefaultReplyUri()
        {
            return "stub://replies".ToUri();
        }

        public IEnumerable<StubMessageCallback> CallbackHistory()
        {
            return Channels.SelectMany(x => x.Callbacks);
        }

        public StubMessageCallback LastCallback()
        {
            return CallbackHistory().Last();
        }

        public Uri ReplyUriFor(Uri node)
        {
            return ReplyChannel.Address;
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
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public StubChannel(Uri address)
        {
            Address = address;
        }

        public bool WasDisposed { get; set; }

        public Uri Address { get; }

        public IReceiver Receiver { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public void StartReceiving(IReceiver receiver)
        {
            Receiver = receiver;
        }

        public async Task Send(byte[] data, IDictionary<string, string> headers)
        {
            var callback = new StubMessageCallback(this);
            Callbacks.Add(callback);

            await Receiver?.Receive(data, headers, callback);
        }
    }

    public class StubMessageCallback : IMessageCallback
    {
        private readonly StubChannel _channel;
        public readonly IList<ErrorReport> Errors = new List<ErrorReport>();
        public readonly IList<Envelope> Sent = new List<Envelope>();

        public StubMessageCallback(StubChannel channel)
        {
            _channel = channel;
        }

        public bool MarkedSucessful { get; set; }

        public Exception Exception { get; set; }

        public bool MarkedFailed { get; set; }

        public DateTime? DelayedTo { get; set; }

        public bool WasMovedToErrors { get; set; }

        public bool Requeued { get; set; }

        public void MarkSuccessful()
        {
            MarkedSucessful = true;
        }

        public void MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
        }

        public Task MoveToDelayedUntil(DateTime time)
        {
            DelayedTo = time;
            return Task.CompletedTask;
        }

        public void MoveToErrors(ErrorReport report)
        {
            WasMovedToErrors = true;
            Errors.Add(report);
        }

        public Task Requeue(Envelope envelope)
        {
            Requeued = true;
            return _channel.Send(envelope.Data, envelope.Headers);
        }

        public Task Send(Envelope envelope)
        {
            Sent.Add(envelope);
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = false;
    }
}