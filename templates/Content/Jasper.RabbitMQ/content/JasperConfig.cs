using Jasper;
using Jasper.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace JasperService
{
    internal class JasperConfig : JasperOptions
    {
        public JasperConfig()
        {
            // Listen for incoming messages at a Rabbit MQ
            // queue
            Endpoints
                .ListenToRabbitQueue("Incoming")
                
                // Explicitly make this queue be designated for reply
                // messages from other Jasper applications
                .UseForReplies();

            // All outgoing messages should be published to 
            // the designated Rabbit MQ routing key / queue / topic
            Endpoints.PublishAllMessages().ToRabbit("Topic1");
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            // Configure Rabbit MQ connections and optionally declare Rabbit MQ
            // objects through an extension method on JasperOptions.Endpoints
            Endpoints.ConfigureRabbitMq(rabbit =>
            {
                // When running in development environments, declare all necessary
                // Rabbit MQ exchanges, queues, and binding keys as part of application
                // startup. This assumes that the developer has permission to do this
                // with a
                if (hosting.IsDevelopment())
                {
                    rabbit.AutoProvision = true;

                    // When running in development, the Jasper team prefers
                    // to just use Rabbit MQ running in Docker on a developer's
                    // workstation
                    rabbit.ConnectionFactory.HostName = "localhost";
                }
                else
                {
                    // The connection is the only thing that is absolutely mandatory
                    rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("RabbitMqUri");
                }

                // Declare any required Rabbit MQ objects. This is strictly
                // for the auto-provisioning at start up and has no impact on
                // the functionality

                // Declare an exchange
                rabbit.DeclareExchange("RabbitExchange");

                // Declare any queues
                rabbit.DeclareQueue("Incoming");
                rabbit.DeclareQueue("Outgoing");

                // Declare any required Rabbit MQ bindings
                // or topics
                rabbit.DeclareBinding(new Binding
                {
                    BindingKey = "Key1",
                    QueueName = "Outgoing",
                    ExchangeName = "RabbitExchange"

                });
            });
        }
    }

}