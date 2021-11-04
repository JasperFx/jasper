using System;
using System.Threading.Tasks;
using Baseline;
using Jasper.Attributes;
using Jasper.AzureServiceBus.Internal;
using Jasper.Transports;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Jasper.AzureServiceBus.Tests
{
    // SAMPLE: SettingAzureServiceBusOptions
    public class JasperWithAzureServiceBusApp : JasperOptions
    {
        public JasperWithAzureServiceBusApp()
        {
            Endpoints.ConfigureAzureServiceBus(asb =>
            {
                asb.ConnectionString = "an Azure Service Bus connection string";

                // The following properties would be set on all
                // TopicClient, QueueClient, or SubscriptionClient
                // objects created at runtime
                asb.TransportType = TransportType.AmqpWebSockets;
                asb.TokenProvider = new ManagedServiceIdentityTokenProvider();
                asb.ReceiveMode = ReceiveMode.ReceiveAndDelete;
                asb.RetryPolicy = RetryPolicy.NoRetry;
            });

            // Configure endpoints
            Endpoints.PublishAllMessages().ToAzureServiceBusQueue("outgoing");
            Endpoints.ListenToAzureServiceBusQueue("incoming");
        }
    }
    // ENDSAMPLE

    // SAMPLE: AzureServiceBus-AzureServiceBusTopicSendingApp
    public class AzureServiceBusTopicSendingApp : JasperOptions
    {
        public AzureServiceBusTopicSendingApp()
        {
            Endpoints.ConfigureAzureServiceBus(asb =>
            {
                asb.ConnectionString = "an Azure Service Bus connection string";
            });

            // This directs Jasper to send all messages to
            // an Azure Service Bus topic name derived from the
            // message type
            Endpoints.PublishAllMessages()
                .ToAzureServiceBusTopics();
        }
    }
    // ENDSAMPLE


    public class MySpecialProtocol : ITransportProtocol<Message>
    {
        public Message WriteFromEnvelope(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Envelope ReadEnvelope(Message message)
        {
            throw new NotImplementedException();
        }
    }


    // SAMPLE: PublishAndSubscribeToAzureServiceBusQueue
    internal class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            // Publish all messages to an Azure Service Bus queue
            Endpoints
                .PublishAllMessages()
                .ToAzureServiceBusQueue("pings")

                // Optionally use the store and forward
                // outbox mechanics against this endpoint
                .Durably();

            // Listen to incoming messages from an Azure Service Bus
            // queue
            Endpoints
                .ListenToAzureServiceBusQueue("pongs");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE

    // SAMPLE: PublishAndSubscribeToAzureServiceBusQueueByUri
    internal class JasperConfig2 : JasperOptions
    {
        public JasperConfig2()
        {
            // Publish all messages to an Azure Service Bus queue
            Endpoints
                .PublishAllMessages()
                .To("asb://queue/pings");


            // Listen to incoming messages from an Azure Service Bus
            // queue
            Endpoints
                .ListenForMessagesFrom("asb://queue/pongs")
                .UseForReplies();
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE


    // SAMPLE: PublishAndSubscribeToAzureServiceBusTopic
    internal class JasperConfig3 : JasperOptions
    {
        public JasperConfig3()
        {
            // Publish all messages to an Azure Service Bus queue
            Endpoints
                .PublishAllMessages()
                .ToAzureServiceBusTopic("pings")

                // Optionally use the store and forward
                // outbox mechanics against this endpoint
                .Durably();

            // Listen to incoming messages from an Azure Service Bus
            // queue
            Endpoints
                .ListenToAzureServiceBusTopic("pongs", "pong-subscription");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE

    // SAMPLE: PublishAndSubscribeToAzureServiceBusTopicByUri
    internal class JasperConfig4 : JasperOptions
    {
        public JasperConfig4()
        {
            // Publish all messages to an Azure Service Bus queue
            Endpoints
                .PublishAllMessages()
                .To("asb://topic/pings");


            // Listen to incoming messages from an Azure Service Bus
            // queue
            Endpoints
                .ListenForMessagesFrom("asb://topic/pongs/subscription/pong-subscription")
                .UseForReplies();
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE


    // SAMPLE: CustomAzureServiceBusProtocol

    public class SpecialAzureServiceBusProtocol : DefaultAzureServiceBusProtocol
    {
        public override Message WriteFromEnvelope(Envelope envelope)
        {
            var message = base.WriteFromEnvelope(envelope);

            // Override some properties from how
            // Jasper itself would write them out
            message.ReplyTo = "replies";

            return message;
        }

        public override Envelope ReadEnvelope(Message message)
        {
            var envelope = base.ReadEnvelope(message);

            if (message.ReplyTo.IsNotEmpty())
            {
                var uriString = "asb://topic/" + message.ReplyTo;
            }

            return envelope;
        }
    }
    // ENDSAMPLE


    // SAMPLE: PublishAndSubscribeToAzureServiceBusTopicAndCustomProtocol
    internal class JasperConfig5 : JasperOptions
    {
        public JasperConfig5()
        {
            // Publish all messages to an Azure Service Bus queue
            Endpoints
                .PublishAllMessages()
                .ToAzureServiceBusTopic("pings")

                // Override the Azure Service Bus protocol
                // because it's not a Jasper application on the other end
                .Protocol<SpecialAzureServiceBusProtocol>()

                // Optionally use the store and forward
                // outbox mechanics against this endpoint
                .Durably();

            // Listen to incoming messages from an Azure Service Bus
            // queue
            Endpoints
                .ListenToAzureServiceBusTopic("pongs", "pong-subscription");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE


    // SAMPLE: ItemCreatedWithTopic
    [Topic("items")]
    public class ItemCreated
    {
        public string Name { get; set; }
    }
    // ENDSAMPLE

    public static class Sender
    {
        // SAMPLE: SendItemCreatedByTopic
        public static async Task SendMessage(IMessagePublisher publisher)
        {
            await publisher.Send(new ItemCreated
            {
                Name = "NewItem"
            });
        }
        // ENDSAMPLE

        // SAMPLE: SendItemCreatedToTopic
        public static async Task SendToTopic(IMessagePublisher publisher)
        {
            var @event = new ItemCreated
            {
                Name = "New Thing"
            };

            // This call sends the ItemCreated message to the
            // "NorthAmerica" topic
            await publisher.SendToTopic(@event, "NorthAmerica");
        }
        // ENDSAMPLE

        // SAMPLE: SendLogMessageToTopic
        public static async Task SendLogMessage(IMessagePublisher publisher)
        {
            var message = new LogMessage
            {
                Message = "Watch out!",
                Priority = "High"
            };

            // In this sample, Jasper will route the LogMessage
            // message to the "High" topic
            await publisher.Send(message);
        }
        // ENDSAMPLE
    }

    // SAMPLE: LogMessageWithPriority
    public class LogMessage
    {
        public string Message { get; set; }
        public string Priority { get; set; }
    }
    // ENDSAMPLE

    // SAMPLE: AppWithTopicNamingRule
    public class PublishWithTopicRulesApp : JasperOptions
    {
        public PublishWithTopicRulesApp()
        {
            Endpoints.PublishAllMessages()
                .ToAzureServiceBusTopics()

                // This is setting up a topic name rule
                // for any message of type that can be
                // cast to LogMessage
                .OutgoingTopicNameIs<LogMessage>(x => x.Priority);
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("azureservicebus");
            Endpoints.ConfigureAzureServiceBus(connectionString);
        }
    }
    // ENDSAMPLE
}
