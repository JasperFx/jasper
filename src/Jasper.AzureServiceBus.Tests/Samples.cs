using System;
using Baseline;
using Jasper.AzureServiceBus.Internal;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#if NETSTANDARD2_0
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
#else
using IHostEnvironment = Microsoft.Extensions.Hosting.IHostEnvironment;
#endif

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


    public class MySpecialProtocol : IAzureServiceBusProtocol
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
}
