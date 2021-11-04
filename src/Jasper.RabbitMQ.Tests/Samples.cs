﻿using System;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Tests
{
    // SAMPLE: AppListeningToRabbitMQ
    public class AppListeningToRabbitMQ : JasperOptions
    {
        public AppListeningToRabbitMQ()
        {
            // Port is optional if you're using the default RabbitMQ
            // port of 5672, but shown here for completeness
            //Transports.ListenForMessagesFrom("rabbitmq://rabbitserver:5672/messages");
        }
    }
    // ENDSAMPLE

    // SAMPLE: AppPublishingToRabbitMQ
    public class AppPublishingToRabbitMQ : JasperOptions
    {
        public AppPublishingToRabbitMQ()
        {
            //Publish.AllMessagesTo("rabbitmq://rabbitserver:5672/messages");
        }
    }
    // ENDSAMPLE


    // SAMPLE: CustomizedRabbitMQApp
    public class CustomizedRabbitMQApp : JasperOptions
    {
        public CustomizedRabbitMQApp()
        {
//            Settings.Alter<RabbitMqOptions>(settings =>
//            {
//                // Retrieve the Jasper "agent" by the full Uri:
//                settings.ConfigureEndpoint("rabbitmq://connection1/queue/queue1", endpoint =>
//                {
//                    // Customize the underlying ConnectionFactory for security mechanisms,
//                    // timeouts, and many other settings
//                    endpoint.ConnectionFactory.ContinuationTimeout = TimeSpan.FromSeconds(5);
//
//
//                    // Customize or change how Jasper maps Envelopes to and from
//                    // the RabbitMQ properties
//                    endpoint.Protocol = new CustomRabbitMqProtocol();
//                });
//            });
        }
    }

    public class CustomRabbitMqProtocol : DefaultRabbitMqProtocol
    {
        public override void ReadIntoEnvelope(Envelope envelope, IBasicProperties props, byte[] data)
        {
            // Customize the mappings from RabbitMQ headers to
            // Jasper's Envelope values
            base.ReadIntoEnvelope(envelope, props, data);

            envelope.Source = "CustomSystem";
        }

        public override void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            // Customize how Jasper Envelope objects are mapped to
            // the outgoing RabbitMQ message structure
            base.WriteFromEnvelope(envelope, properties);
        }
    }

    // ENDSAMPLE
}
