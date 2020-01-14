using System;
using System.Net;
using Jasper;
using Jasper.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using ExchangeType = Jasper.RabbitMQ.ExchangeType;

namespace IntegrationTests.Samples
{


    public class MySpecialProtocol : IRabbitMqProtocol
    {
        public void WriteFromEnvelope(Envelope envelope, IBasicProperties properties)
        {
            throw new System.NotImplementedException();
        }

        public Envelope ReadEnvelope(byte[] body, IBasicProperties properties)
        {
            throw new System.NotImplementedException();
        }
    }


    // SAMPLE: PublishAndListenForRabbitMqQueue
    internal class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            Endpoints
                // Listen for messages incoming on a specific
                // named queue
                .ListenToRabbitQueue("pongs")

                // With the Rabbit MQ transport, you probably
                // want to explicitly designate a specific queue or topic
                // for replies
                .UseForReplies();

            Endpoints.PublishAllMessages()

                // The argument here can be either a queue
                // name or a routing key. It's the same as far
                // as the Rabbit MQ .Net Client is concerned
                .ToRabbit("pings");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            Endpoints.ConfigureRabbitMq(rabbit =>
            {
                rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("rabbit");
            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: PublishAndListenForRabbitMqQueueByUri
    internal class JasperConfig2 : JasperOptions
    {
        public JasperConfig2()
        {
            Endpoints
                // Listen for messages incoming on a specific
                // named queue
                .ListenToRabbitQueue("rabbitmq://queue/pongs")



                // With the Rabbit MQ transport, you probably
                // want to explicitly designate a specific queue or topic
                // for replies
                .UseForReplies();


            Endpoints.PublishAllMessages()
                // To a specific queue
                .To("rabbitmq://queue/pings");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            Endpoints.ConfigureRabbitMq(rabbit =>
            {
                rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("rabbit");
            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: PublishRabbitMqTopic
    internal class JasperConfig3 : JasperOptions
    {
        public JasperConfig3()
        {
            Endpoints
                // Listen for messages incoming on a specific
                // named queue
                .ListenToRabbitQueue("pongs")

                // With the Rabbit MQ transport, you probably
                // want to explicitly designate a specific queue or topic
                // for replies
                .UseForReplies();

            Endpoints.PublishAllMessages()

                // The argument here can be either a queue
                // name or a routing key. It's the same as far
                // as the Rabbit MQ .Net Client is concerned
                .ToRabbit("pings", "topics");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            Endpoints.ConfigureRabbitMq(rabbit =>
            {
                rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("rabbit");

                if (hosting.IsDevelopment())
                {
                    rabbit.AutoProvision = true;
                }

                // This is optional, but does help for local development
                rabbit.DeclareExchange("topics", exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Topic;
                });

                rabbit.DeclareQueue("incoming-pings", q =>
                {
                    // Just showing that it's possible to further configure
                    // the queue
                    q.IsDurable = true;
                });

                // This would more likely be on the listener side,
                // but just showing you what can be done
                rabbit.DeclareBinding(new Binding
                {
                    BindingKey = "pings",
                    ExchangeName = "topics",
                    QueueName = "incoming-pings"
                });
            });
        }
    }
    // ENDSAMPLE


    // SAMPLE: PublishRabbitMqFanout
    internal class JasperConfig4 : JasperOptions
    {
        public JasperConfig4()
        {
            Endpoints
                // Listen for messages incoming on a specific
                // named queue
                .ListenToRabbitQueue("pongs")

                // With the Rabbit MQ transport, you probably
                // want to explicitly designate a specific queue or topic
                // for replies
                .UseForReplies();

            // Publish to the exchange name
            Endpoints.PublishAllMessages()
                .ToRabbitExchange("fan");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            Endpoints.ConfigureRabbitMq(rabbit =>
            {
                rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("rabbit");

                if (hosting.IsDevelopment())
                {
                    rabbit.AutoProvision = true;
                }

                // This is optional
                rabbit.DeclareExchange("fan", exchange =>
                {
                    exchange.ExchangeType = ExchangeType.Fanout;
                });

                // This would more likely be on the listener side,
                // but just showing you what can be done
                rabbit.DeclareBinding(new Binding
                {
                    BindingKey = "pings",
                    ExchangeName = "fan",
                    QueueName = "incoming-pings"
                });
            });
        }
    }
    // ENDSAMPLE

}
