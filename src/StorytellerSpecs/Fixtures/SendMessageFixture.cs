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
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
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
                    try
                    {
                        _runtime.Get<IDurableMessagingFactory>().ClearAllStoredMessages();
                    }
                    catch (Exception)
                    {
                        // too flaky in windows, and this is only for testing
                    }
                });
        }

        [FormatAs("Send message {messageType} named {name}")]
        public void SendMessage([SelectionList("MessageTypes")] string messageType, string name)
        {
            var history = _runtime.Get<MessageHistory>();

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(() =>
            {
                _runtime.Get<IMessageContext>().Send(message).Wait();
            });

            waiter.Wait(5.Seconds());

            StoryTellerAssert.Fail(!waiter.IsCompleted, "Messages were never completely tracked");
        }

        [FormatAs("Send message {messageType} named {name} directly to {address}")]
        public void SendMessageDirectly([SelectionList("MessageTypes")] string messageType, string name,
            [SelectionList("Channels")] Uri address)
        {
            var history = _runtime.Get<MessageHistory>();

            var type = messageTypeFor(messageType);
            var message = Activator.CreateInstance(type).As<Message>();
            message.Name = name;

            var waiter = history.Watch(async () => { await bus().Send(address, message).ConfigureAwait(false); });

            waiter.Wait(5.Seconds());
        }

        private IMessageContext bus()
        {
            return _runtime.Get<IMessageContext>();
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
                _runtime.Get<MessageTracker>().Records
                    .Add(new MessageRecord("Some Service", "stub://replies".ToUri(), t.Result));
            });
        }

        private IList<MessageRecord> sent()
        {
            return _runtime.Get<MessageTracker>().Records.Select(x =>
            {
                x.ReceivedAt = x.ReceivedAt.ToString().Replace("127.0.0.1", "localhost").ToUri();

                return x;
            }).ToList();
        }

        [FormatAs("Send a Message1 named 'Ack' that we expect to succeed and wait for the ack")]
        public void SendMessageSuccessfully()
        {
            _task = bus().SendAndWait(new Message1 {Name = "Ack"});
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
            catch (Exception)
            {
                // swallow the ex for the sake of the test
            }
            return _task.IsCompleted || _task.IsFaulted;
        }

        [FormatAs("The acknowledgement was successful")]
        public bool AckWasSuccessful()
        {
            StoryTellerAssert.Fail(_task.IsFaulted || !_task.IsCompleted,
                () => _task.Exception?.ToString() ?? "Task was not completed");

            return true;
        }

        [FormatAs("The acknowledgment failed and contained the message {message}")]
        public bool TheAckFailedWithMessage(string message)
        {
            StoryTellerAssert.Fail(_task.Exception == null, "The task exception is null");

            StoryTellerAssert.Fail(!_task.Exception.InnerExceptions.First().ToString().Contains(message),
                "The actual exception text was:\n" + _task.Exception);

            return true;
        }

        [FormatAs("Send a message with an unknown content type to {address}")]
        public async Task SendMessageWithUnknownContentType([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = Envelope.ForData(bytes, null);
            envelope.ContentType = "text/xml";

            envelope.Destination = address;

            var sender = _runtime.Get<IMessageContext>();
            await sender.Send(envelope);
        }

        [FormatAs("Send a garbled message to {address}")]
        public async Task SendGarbledMessage([SelectionList("Channels")] Uri address)
        {
            var bytes = Encoding.UTF8.GetBytes("<garbage/>");
            var envelope = Envelope.ForData(bytes, null);

            envelope.ContentType = "application/json";

            envelope.Destination = address;

            var sender = _runtime.Get<IMessageContext>();
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
            return uri.Host.EqualsIgnoreCase(Environment.MachineName)
                ? new UriBuilder(uri) {Host = "localhost"}.Uri
                : uri;
        }
    }
}
