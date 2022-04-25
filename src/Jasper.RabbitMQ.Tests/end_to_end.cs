using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using Baseline.ImTools;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ.Internal;
using Jasper.Tracking;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Oakton.Resources;
using Shouldly;
using TestingSupport;
using Weasel.Core;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public static class RabbitTesting
    {
        public static int Number;

        public static string NextQueueName()
        {
            return $"messages{++Number}";
        }

        public static string NextExchangeName()
        {
            return $"exchange{++Number}";
        }
    }

    public class end_to_end
    {
        [Fact]
        public void rabbitmq_transport_is_exposed_as_a_resource()
        {
            var queueName = RabbitTesting.NextQueueName();
            using var publisher = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq().AutoProvision().AutoPurgeOnStartup();

                opts.PublishAllMessages()
                    .ToRabbitQueue(queueName)
                    .Durably();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();

                opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            });

            publisher.Services.GetServices<IStatefulResourceSource>().SelectMany(x => x.FindResources())
                .OfType<RabbitMqTransport>().Any().ShouldBeTrue();
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var queueName = RabbitTesting.NextQueueName();
            using var publisher = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq().AutoProvision().AutoPurgeOnStartup();

                opts.PublishAllMessages()
                    .ToRabbitQueue(queueName)
                    .Durably();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();

                opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            });


            using var receiver = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq().AutoProvision();

                opts.ListenToRabbitQueue(queueName);
                opts.Services.AddSingleton<ColorHistory>();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "receiver";
                }).IntegrateWithJasper();

                opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            });

            await receiver.ResetResourceState();

            await publisher
                .TrackActivity()
                .AlsoTrack(receiver)
                .Timeout(30.Seconds()) // this one can be slow when it's in a group of tests
                .SendMessageAndWait(new ColorChosen { Name = "Orange" });


            receiver.Get<ColorHistory>().Name.ShouldBe("Orange");
        }


        [Fact]
        public async Task reply_uri_mechanics()
        {
            var queueName1 = RabbitTesting.NextQueueName();
            var queueName2 = RabbitTesting.NextQueueName();


            using var publisher = JasperHost.For(opts =>
            {
                opts.ServiceName = "Publisher";
                opts.Extensions.UseMessageTrackingTestingSupport();


                opts.UseRabbitMq().AutoProvision();

                opts.PublishAllMessages()
                    .ToRabbitQueue(queueName1)
                    .Durably();

                opts.ListenToRabbitQueue(queueName2).UseForReplies();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "sender";
                }).IntegrateWithJasper();

                opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            });

            using var receiver = JasperHost.For(opts =>
            {
                opts.ServiceName = "Receiver";

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq().AutoProvision();

                opts.ListenToRabbitQueue(queueName1);
                opts.Services.AddSingleton<ColorHistory>();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "receiver";
                }).IntegrateWithJasper();

                opts.Advanced.StorageProvisioning = StorageProvisioning.Rebuild;
            });

            var session = await publisher
                .TrackActivity()
                .AlsoTrack(receiver)
                .SendMessageAndWait(new PingMessage { Number = 1 });


            // TODO -- let's make an assertion here?
            var records = session.FindEnvelopesWithMessageType<PongMessage>(EventType.Received);
            records.Any(x => x.ServiceName == "Publisher").ShouldBeTrue();
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_routing_key()
        {
            var queueName = RabbitTesting.NextQueueName();
            var exchangeName = RabbitTesting.NextExchangeName();

            var publisher = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq()
                    .AutoProvision()
                    .BindExchange(exchangeName)
                    .ToQueue(queueName, "key2");

                opts.PublishAllMessages().ToRabbit("key2", exchangeName);
            });

            var receiver = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq()
                    .AutoProvision()
                    .DeclareQueue(RabbitTesting.NextQueueName())
                    .BindExchange(exchangeName).ToQueue(queueName, "key2");

                opts.Services.AddSingleton<ColorHistory>();

                opts.ListenToRabbitQueue(queueName);
            });

            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .SendMessageAndWait(new ColorChosen { Name = "Orange" });

                receiver.Get<ColorHistory>().Name.ShouldBe("Orange");
            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }


        [Fact]
        public async Task schedule_send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var queueName = RabbitTesting.NextQueueName();

            var publisher = JasperHost.For(opts =>
            {
                opts.Advanced.ScheduledJobFirstExecution = 1.Seconds();
                opts.Advanced.ScheduledJobPollingTime = 1.Seconds();
                opts.ServiceName = "Publisher";

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq().AutoProvision().AutoPurgeOnStartup();

                opts.PublishAllMessages().ToRabbitQueue(queueName).Durably();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "rabbit_sender";
                }).IntegrateWithJasper();
            });

            await publisher.ResetResourceState();

            var receiver = JasperHost.For(opts =>
            {
                opts.ServiceName = "Receiver";

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.UseRabbitMq();

                opts.ListenToRabbitQueue(queueName);
                opts.Services.AddSingleton<ColorHistory>();

                opts.Services.AddMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "rabbit_receiver";
                }).IntegrateWithJasper();
            });

            await receiver.ResetResourceState();

            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .WaitForMessageToBeReceivedAt<ColorChosen>(receiver)
                    .Timeout(15.Seconds())
                    .ExecuteAndWait(c => c.ScheduleSendAsync(new ColorChosen { Name = "Orange" }, 5.Seconds()));

                receiver.Get<ColorHistory>().Name.ShouldBe("Orange");
            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }


        [Fact]
        public async Task use_fan_out_exchange()
        {
            var exchangeName = "fanout";
            var queueName1 = RabbitTesting.NextQueueName() + "e23";
            var queueName2 = RabbitTesting.NextQueueName() + "e23";
            var queueName3 = RabbitTesting.NextQueueName() + "e23";


            var publisher = JasperHost.For(opts =>
            {
                opts.UseRabbitMq().AutoProvision()
                    .BindExchange(exchangeName).ToQueue(queueName1)
                    .BindExchange(exchangeName).ToQueue(queueName2)
                    .BindExchange(exchangeName).ToQueue(queueName3);

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.PublishAllMessages().ToRabbitExchange(exchangeName);
            });

            var receiver1 = JasperHost.For(opts =>
            {
                opts.UseRabbitMq();

                opts.Extensions.UseMessageTrackingTestingSupport();
                opts.ListenToRabbitQueue(queueName1);
                opts.Services.AddSingleton<ColorHistory>();
            });

            var receiver2 = JasperHost.For(opts =>
            {
                opts.UseRabbitMq();


                opts.Extensions.UseMessageTrackingTestingSupport();
                opts.ListenToRabbitQueue(queueName2);
                opts.Services.AddSingleton<ColorHistory>();
            });

            var receiver3 = JasperHost.For(opts =>
            {
                opts.UseRabbitMq();


                opts.Extensions.UseMessageTrackingTestingSupport();
                opts.ListenToRabbitQueue(queueName3);
                opts.Services.AddSingleton<ColorHistory>();
            });

            try
            {
                var session = await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver1, receiver2, receiver3)
                    .WaitForMessageToBeReceivedAt<ColorChosen>(receiver1)
                    .WaitForMessageToBeReceivedAt<ColorChosen>(receiver2)
                    .WaitForMessageToBeReceivedAt<ColorChosen>(receiver3)
                    .SendMessageAndWait(new ColorChosen { Name = "Purple" });


                receiver1.Get<ColorHistory>().Name.ShouldBe("Purple");
                receiver2.Get<ColorHistory>().Name.ShouldBe("Purple");
                receiver3.Get<ColorHistory>().Name.ShouldBe("Purple");
            }
            finally
            {
                publisher.Dispose();
                receiver1.Dispose();
                receiver2.Dispose();
                receiver3.Dispose();
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_named_topic()
        {
            var queueName = RabbitTesting.NextQueueName();

            var publisher = JasperHost.For(opts =>
            {
                opts.UseRabbitMq().AutoProvision()
                    .BindExchange("topics", ExchangeType.Topic)
                    .ToQueue(queueName, "special");

                opts.PublishAllMessages().ToRabbit("special", "topics");

                opts.Handlers.DisableConventionalDiscovery();

                opts.Extensions.UseMessageTrackingTestingSupport();
            });

            var receiver = JasperHost.For(opts =>
            {
                opts.UseRabbitMq();

                opts.ListenToRabbitQueue(queueName);

                opts.Extensions.UseMessageTrackingTestingSupport();

                opts.Handlers.DisableConventionalDiscovery().IncludeType<SpecialTopicGuy>();
            });

            try
            {
                var message = new SpecialTopic();
                var session = await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .SendMessageAndWait(message);


                var received = session.FindSingleTrackedMessageOfType<SpecialTopic>(EventType.MessageSucceeded);
                received
                    .Id.ShouldBe(message.Id);
            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }
    }

    public class SpecialTopicGuy
    {
        public void Handle(SpecialTopic topic)
        {
        }
    }

    public class ColorHandler
    {
        public void Handle(ColorChosen message, ColorHistory history, Envelope envelope)
        {
            history.Name = message.Name;
            history.Envelope = envelope;
        }
    }

    public class ColorHistory
    {
        public string Name { get; set; }
        public Envelope Envelope { get; set; }
    }

    public class ColorChosen
    {
        public string Name { get; set; }
    }


    [MessageIdentity("A")]
    public class TopicA
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    [MessageIdentity("B")]
    public class TopicB
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    [MessageIdentity("C")]
    public class TopicC
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    public class SpecialTopic
    {
        public Guid Id { get; set; } = Guid.NewGuid();
    }

    // The [MessageIdentity] attribute is only necessary
    // because the projects aren't sharing types
    // You would not do this if you were distributing
    // message types through shared assemblies
    [MessageIdentity("TryToReconnect")]
    public class PingMessage
    {
        public int Number { get; set; }
    }

    [MessageIdentity("Pong")]
    public class PongMessage
    {
        public int Number { get; set; }
    }

    public static class PongHandler
    {
        // "Handle" is recognized by Jasper as a message handling
        // method. Handler methods can be static or instance methods
        public static void Handle(PongMessage message)
        {
            ConsoleWriter.Write(ConsoleColor.Blue, $"Got pong #{message.Number}");
        }
    }

    public static class PingHandler
    {
        // Simple message handler for the PingMessage message type
        public static Task Handle(
            // The first argument is assumed to be the message type
            PingMessage message,

            // Jasper supports method injection similar to ASP.Net Core MVC
            // In this case though, IMessageContext is scoped to the message
            // being handled
            IExecutionContext context)
        {
            ConsoleWriter.Write(ConsoleColor.Blue, $"Got ping #{message.Number}");

            var response = new PongMessage
            {
                Number = message.Number
            };

            // This usage will send the response message
            // back to the original sender. Jasper uses message
            // headers to embed the reply address for exactly
            // this use case
            return context.RespondToSender(response);
        }
    }
}
