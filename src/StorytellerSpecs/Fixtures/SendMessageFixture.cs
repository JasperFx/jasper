using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Durability;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Util;
using Microsoft.Extensions.Hosting;
using StoryTeller;

namespace StorytellerSpecs.Fixtures
{
    public class SendMessageFixture : BusFixture
    {
        private IHost _host;
        private ITrackedSession _session;

        public SendMessageFixture()
        {
            Title = "Send Messages through the Service Bus";
        }


        public IGrammar IfTheApplicationIs()
        {
            return Embed<ServiceBusApplication>("If a service bus application is configured to")
                .After(c =>
                {
                    _host = c.State.Retrieve<IHost>();
                    try
                    {
                        _host.Get<IEnvelopePersistence>().Admin.ClearAllPersistedEnvelopes();
                    }
                    catch (Exception)
                    {
                        // too flaky in windows, and this is only for testing
                    }
                });
        }

        [FormatAs("Send message {messageType} named {name}")]
        public async Task SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            _session = await _host.TrackActivity().IncludeExternalTransports().SendMessageAndWait(message);
        }

        [FormatAs("Send message {messageType} named {name} directly to {address}")]
        public async Task SendMessageDirectly([SelectionList("MessageTypes")] string messageType, string name,
            [SelectionList("Channels")] Uri address)
        {
            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            _session = await _host.TrackActivity().IncludeExternalTransports().ExecuteAndWait(x => x.Send(address, message));

        }

        public IGrammar TheMessagesSentShouldBe()
        {
            return VerifySetOf(sent).Titled("All the messages sent should be")
                .MatchOn(x => x.ReceivedAt, x => x.MessageType, x => x.Name);
        }

        private IList<MessageRecord> sent()
        {
            return _session.AllRecordsInOrder(EventType.Received).Select(x =>
                new MessageRecord(x.ServiceName, x.Envelope.Destination, (Message) x.Envelope.Message)).ToList();

        }


        [FormatAs("Send a message with an unknown content type to {address}")]
        public async Task SendMessageWithUnknownContentType([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = Envelope.ForData(bytes, null);
            envelope.Message = new Garbled();
            envelope.ContentType = "text/xml";

            envelope.Destination = address;

            var sender = _host.Get<IMessageContext>();
            await sender.Send(envelope);
        }

        [FormatAs("Send a garbled message to {address}")]
        public async Task SendGarbledMessage([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = Envelope.ForData(bytes, null);
            envelope.Message = new Garbled();

            envelope.ContentType = "application/json";

            envelope.Destination = address;

            _session = await _host.TrackActivity().DoNotAssertOnExceptionsDetected().IncludeExternalTransports()
                .ExecuteAndWait(x => x.SendEnvelope(envelope));
        }

        public override void TearDown()
        {
            _host.Dispose();
        }
    }

    public class Garbled
    {
    }

    [MessageIdentity("ErrorMessage")]
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

    [MessageIdentity("Message1")]
    public class Message1 : Message
    {
    }

    [MessageIdentity("Message2")]
    public class Message2 : Message
    {
    }

    [MessageIdentity("Message3")]
    public class Message3 : Message
    {
    }

    [MessageIdentity("Message4")]
    public class Message4 : Message
    {
    }

    [MessageIdentity("Message5")]
    public class Message5 : Message
    {
    }

    [MessageIdentity("Message6")]
    public class Message6 : Message
    {
    }

    public class Cascader1
    {
        public Message2 Handle(Message1 message)
        {
            return new Message2 {Name = message.Name};
        }
    }

    public class Cascader2
    {
        public (Message3, Message4) Handle(Message2 message)
        {
            return (new Message3 {Name = message.Name}, new Message4 {Name = message.Name});
        }
    }

    public abstract class MessageHandler<T> where T : Message
    {
        public void Handle(T message, MessageTracker tracker, Envelope envelope, JasperOptions options)
        {
            tracker.Records.Add(new MessageRecord(options.ServiceName, envelope.ReceivedAt, message));
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
            return uri.Host.EqualsIgnoreCase(Environment.MachineName)
                ? new UriBuilder(uri) {Host = "localhost"}.Uri
                : uri;
        }
    }
}
