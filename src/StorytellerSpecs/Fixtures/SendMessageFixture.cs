using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Authentication.ExtendedProtection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using BlueMilk.Scanning;
using Jasper;
using Jasper.Bus;
using Jasper.Bus.Configuration;
using Jasper.Bus.Delayed;
using Jasper.Bus.Runtime;
using Jasper.Bus.Runtime.Invocation;
using Jasper.Bus.Runtime.Subscriptions;
using Jasper.Bus.Tracking;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Configuration;
using Jasper.Bus.Transports.Core;
using Jasper.Bus.Transports.Durable;
using Jasper.Util;
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


        public IGrammar IfTheApplicationIs()
        {
            return Embed<ServiceBusApplication>("If a service bus application is configured to")
                .After(c =>
                {
                    _runtime = c.State.Retrieve<JasperRuntime>();
                    _runtime.Get<IPersistence>().ClearAllStoredMessages();
                });


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
                _runtime.Container.GetInstance<MessageTracker>().Records.Add(new MessageRecord("Some Service","stub://replies".ToUri(), t.Result));
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
            var envelope = new Envelope(bytes, null);
            envelope.ContentType = "text/xml";

            envelope.Destination = address;

            var sender = _runtime.Container.GetInstance<IEnvelopeSender>();
            await sender.Send(envelope);
        }

        [FormatAs("Send a garbled message to {address}")]
        public async Task SendGarbledMessage([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = new Envelope(bytes, null);
            envelope.ContentType = "application/json";

            envelope.Destination = address;

            var sender = _runtime.Container.GetInstance<IEnvelopeSender>();
            await sender.Send(envelope);
        }

        public override void TearDown()
        {
            _runtime.Dispose();
        }
    }

    [MessageAlias("ErrorMessage")]
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

    [MessageAlias("Message1")]
    public class Message1 : Message
    {
    }

    [MessageAlias("Message2")]
    public class Message2 : Message
    {
    }

    [MessageAlias("Message3")]
    public class Message3 : Message
    {
    }

    [MessageAlias("Message4")]
    public class Message4 : Message
    {
    }

    [MessageAlias("Message5")]
    public class Message5 : Message
    {
    }

    [MessageAlias("Message6")]
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
        public void Handle(T message, MessageTracker tracker, Envelope envelope, JasperRuntime runtime)
        {
            tracker.Records.Add(new MessageRecord(runtime.ServiceName, envelope.ReceivedAt, message));
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
        public MessageRecord(string serviceName, Uri receivedAt, Message message)
        {
            ServiceName = serviceName;
            MessageType = message.GetType().Name;
            Name = message.Name;
            ReceivedAt = receivedAt.ToLocalHostUri();
        }

        public string ServiceName { get; set; }

        public Uri ReceivedAt { get; set; }

        public string Name { get; set; }

        public string MessageType { get; set; }
    }

    public static class UriExtensions
    {
        public static Uri ToLocalHostUri(this Uri uri)
        {
            return uri.Host.EqualsIgnoreCase(Environment.MachineName) ? new UriBuilder(uri) {Host = "localhost"}.Uri : uri;
        }
    }


    public class StubTransport : ITransport
    {
        public readonly LightweightCache<Uri, StubChannel> Channels;
        private IHandlerPipeline _pipeline;
        private Uri _replyUri;

        public StubTransport(string scheme = "stub")
        {
            _replyUri = new Uri($"{scheme}://replies");

            Channels =
                new LightweightCache<Uri, StubChannel>(uri => new StubChannel(uri, _replyUri, _pipeline, null));


            ReplyChannel = Channels[_replyUri];
            Protocol = scheme;


        }

        public StubChannel ReplyChannel { get; set; }

        public bool WasDisposed { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }

        public string Protocol { get; }

        public Task Send(Envelope envelope, Uri destination)
        {
            StubChannel tempQualifier = Channels[destination];
            var callback = new StubMessageCallback(tempQualifier);
            tempQualifier.Callbacks.Add(callback);
            envelope.Callback = callback;

            return _pipeline.Invoke(envelope);
        }



        public IChannel[] Start(IHandlerPipeline pipeline, BusSettings settings, OutgoingChannels channels)
        {
            _pipeline = pipeline;

            foreach (var address in settings.KnownSubscribers.Where(x => x.Uri.Scheme == Protocol))
            {
                Channels[address.Uri] = new StubChannel(address.Uri, _replyUri, pipeline, address);
            }

            foreach (var node in settings.Listeners.Where(x => x.Uri.Scheme == Protocol))
            {
                Channels.FillDefault(node.Uri);
            }



            return Channels.GetAll().OfType<IChannel>().ToArray();
        }

        public Uri DefaultReplyUri()
        {
            return "stub://replies".ToUri();
        }

        public TransportState State { get; } = TransportState.Enabled;
        public void Describe(TextWriter writer)
        {
            throw new NotImplementedException();
        }

        public bool Enabled { get; } = true;

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

        public Uri CorrectedAddressFor(Uri address)
        {
            return address;
        }
    }

    public class StubChannel :ChannelBase
    {
        private readonly IHandlerPipeline _pipeline;
        public readonly IList<StubMessageCallback> Callbacks = new List<StubMessageCallback>();

        public StubChannel(Uri address, Uri replyUri, IHandlerPipeline pipeline, SubscriberAddress subscriberAddress) : base(subscriberAddress ?? new SubscriberAddress(address), replyUri)
        {
            _pipeline = pipeline;
            Address = address;
        }

        public bool WasDisposed { get; set; }

        public Uri Address { get; }
        public SubscriberAddress Subscription { get; set; }

        public void Dispose()
        {
            WasDisposed = true;
        }


        protected override Task send(Envelope envelope)
        {
            var callback = new StubMessageCallback(this);
            Callbacks.Add(callback);

            envelope.Callback = callback;

            return _pipeline.Invoke(envelope);

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

        public Task MarkSuccessful()
        {
            MarkedSucessful = true;
            return Task.CompletedTask;
        }

        public Task MarkFailed(Exception ex)
        {
            MarkedFailed = true;
            Exception = ex;
            return Task.CompletedTask;
        }

        public Task MoveToErrors(ErrorReport report)
        {
            WasMovedToErrors = true;
            Errors.Add(report);
            return Task.CompletedTask;
        }

        public Task Requeue(Envelope envelope)
        {
            Requeued = true;


            return _channel.Send(envelope);
        }

        public Task Send(Envelope envelope)
        {
            Sent.Add(envelope);
            return Task.CompletedTask;
        }

        public bool SupportsSend { get; } = false;
        public string TransportScheme { get; } = "stub";
    }



}
