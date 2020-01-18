using System;
using Jasper.RabbitMQ.CommandLine;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqCommandTester
    {
        private RabbitMqInput theInput;

        public RabbitMqCommandTester()
        {
            theInput = new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>()
            };

        }

        [Fact]
        public void can_declare()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();
        }

        [Fact]
        public void can_purge()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();

            new RabbitMqCommand().Execute(new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>(),
                Action = RabbitAction.purge
            }).ShouldBeTrue();
        }

        [Fact]
        public void can_delete()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();

            new RabbitMqCommand().Execute(new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>(),
                Action = RabbitAction.teardown
            }).ShouldBeTrue();
        }

        public class RabbitUsingApp : JasperOptions
        {
            public RabbitUsingApp()
            {
                Endpoints.ConfigureRabbitMq(x =>
                {

                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("queue15");
                    x.DeclareExchange("ex15", y => y.ExchangeType = ExchangeType.Fanout);
                    x.DeclareBinding(new Binding
                    {
                        BindingKey = "key1",
                        ExchangeName = "ex15",
                        QueueName = "queue15"
                    });
                });
            }
        }

        public class SampleRabbitMqApp : JasperOptions
        {
            public SampleRabbitMqApp()
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
                        rabbit.ConnectionFactory.Uri = config.GetValue<Uri>("RabbitMqConnectionString");
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
}
