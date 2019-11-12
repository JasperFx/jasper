using System;
using System.Linq;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ.Internal;
using Jasper.Util;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    [Collection("marten")]
    public class end_to_end : RabbitMQContext
    {
        [Fact]
        public async Task can_stop_and_start()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp>())
            {
                var root = runtime.Get<IMessagingRoot>();
                root.ListeningStatus = ListeningStatus.TooBusy;
                root.ListeningStatus = ListeningStatus.Accepting;


                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }

        }

        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp>())
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }




        [Fact]
        public void use_explicit_replies_if_it_exists()
        {
            using (var runtime = JasperHost.For(x =>
            {

                x.Settings.Alter<RabbitMqOptions>(settings =>
                {
                    settings.Connections.Add("messages3", "host=localhost");
                    settings.Connections.Add("replies", "host=localhost");
                    settings.Connections.Add("replies2", "host=localhost");

                    settings.ReplyUri = new TransportUri("rabbitmq://replies2/queue/replies2");
                });

                x.Transports.ListenForMessagesFrom("rabbitmq://messages3/queue/messages3");

            }))
            {
                var transport = runtime.Get<ITransport[]>().OfType<RabbitMqTransport>().Single();

                var uri = "rabbitmq://replies2/queue/replies2".ToUri();
                transport.ReplyUri.ShouldBe(uri);

                transport.Listeners.Any(x => x.Address == new Uri("rabbitmq://replies2/queue/replies2")).ShouldBeTrue();
            }
        }

        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_using_connection_string()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp2>())
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }

        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var uri = "rabbitmq://localhost/durable/queue/messages2";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");
                _.Publish.AllMessagesTo(uri);

                _.Include<MartenBackedPersistence>();

                _.Settings.ConfigureMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                });
            });

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

                _.Include<MartenBackedPersistence>();

                _.Settings.MartenConnectionStringIs(Servers.PostgresConnectionString);
            });

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Send(new ColorChosen {Name = "Orange"});

                await wait;

                receiver.Get<ColorHistory>().Name.ShouldBe("Orange");
            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_routing_key()
        {
            var uri = "rabbitmq://localhost/queue/messages5/routingkey/key2";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");
                _.Publish.AllMessagesTo(uri);

            });

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();
            });

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Send(new ColorChosen {Name = "Orange"});

                await wait;

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
            var uri = "rabbitmq://localhost:5672/durable/queue/messages11";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");


                _.Publish.AllMessagesTo(uri);

                _.Include<MartenBackedPersistence>();

                _.Settings.ConfigureMarten(x =>
                {
                    x.DatabaseSchemaName = "rabbit_sender";
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                });
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");


                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

                _.Include<MartenBackedPersistence>();

                _.Settings.ConfigureMarten(x =>
                {
                    x.DatabaseSchemaName = "rabbit_receiver";
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                });
            });

            receiver.RebuildMessageStorage();

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Get<IMessagePublisher>().ScheduleSend(new ColorChosen {Name = "Orange"}, 5.Seconds());

                await wait;

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
            var uri = "rabbitmq://localhost/queue/messages";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.ConfigureRabbitMq(settings =>
                {
                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
                });

                _.Publish.AllMessagesTo(uri);

            });

            var receiver1 = JasperHost.For(_ =>
            {
                _.Settings.ConfigureRabbitMq(settings =>
                {
                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
                });

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

            });

            var receiver2 = JasperHost.For(_ =>
            {
                _.Settings.ConfigureRabbitMq(settings =>
                {
                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
                });

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

            });

            var receiver3 = JasperHost.For(_ =>
            {
                _.Settings.ConfigureRabbitMq(settings =>
                {
                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
                });

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

            });

            var wait1 = receiver1.Get<MessageTracker>().WaitFor<ColorChosen>();
            var wait2 = receiver2.Get<MessageTracker>().WaitFor<ColorChosen>();
            var wait3 = receiver3.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Send(new ColorChosen {Name = "Purple"});

                await wait1;
                //await wait2;
                //await wait3;

                receiver1.Get<ColorHistory>().Name.ShouldBe("Purple");
                //receiver2.Get<ColorHistory>().Name.ShouldBe("Purple");
                //receiver3.Get<ColorHistory>().Name.ShouldBe("Purple");
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
            var uri = "rabbitmq://localhost/queue/messages4/topic/special";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");
                _.Publish.AllMessagesTo(uri);
                _.Handlers.DisableConventionalDiscovery();

            });

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<MessageTracker>();

                _.Handlers.DisableConventionalDiscovery().IncludeType<TracksMessage<SpecialTopic>>();

            });

            var wait = receiver.Get<MessageTracker>().WaitFor<SpecialTopic>();

            try
            {
                var message = new SpecialTopic();
                await publisher.Send(message);

                var received = await wait;
                received.Message.ShouldBeOfType<SpecialTopic>()
                    .Id.ShouldBe(message.Id);


            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }






        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_wildcard_topics()
        {
            var uriString = "rabbitmq://localhost/queue/messages8/topic/*";
            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");
                _.Publish.AllMessagesTo(uriString);
                _.Handlers.DisableConventionalDiscovery();

            });

            var receiver1 = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");

                _.Transports.ListenForMessagesFrom(uriString);
                _.Services.AddSingleton<MessageTracker>();

                _.Handlers.DisableConventionalDiscovery()
                    .IncludeType<TracksMessage<TopicA>>()
                    .IncludeType<TracksMessage<TopicB>>();

            });

            var receiver2 = JasperHost.For(_ =>
            {
                _.Settings.AddRabbitMqHost("localhost");

                _.Transports.ListenForMessagesFrom(uriString);
                _.Services.AddSingleton<MessageTracker>();

                _.Handlers.DisableConventionalDiscovery()
                    .IncludeType<TracksMessage<TopicC>>();

            });

            var waitForA = receiver1.Get<MessageTracker>().WaitFor<TopicA>();
            var waitForB = receiver1.Get<MessageTracker>().WaitFor<TopicB>();
            var waitForC = receiver2.Get<MessageTracker>().WaitFor<TopicC>();

            try
            {
                var topicA = new TopicA();
                var topicB = new TopicB();
                var topicC = new TopicC();

                await publisher.Send(topicA);
                await publisher.Send(topicB);
                await publisher.Send(topicC);

                var receivedA = (await waitForA).Message.ShouldBeOfType<TopicA>();
                var receivedB = (await waitForB).Message.ShouldBeOfType<TopicB>();
                var receivedC = (await waitForC).Message.ShouldBeOfType<TopicC>();

            }
            finally
            {
                publisher.Dispose();
                receiver1.Dispose();
                receiver2.Dispose();
            }
        }




    }


    public class RabbitMqUsingApp : JasperOptions
    {
        public RabbitMqUsingApp()
        {
            Settings.AddRabbitMqHost("localhost");


            Transports.ListenForMessagesFrom("rabbitmq://localhost/queue/messages3");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("rabbitmq://localhost/queue/messages3");

            Include<MessageTrackingExtension>();
        }
    }

    public class RabbitMqUsingApp2 : JasperOptions
    {
        public RabbitMqUsingApp2()
        {
            Settings.AddRabbitMqHost("localhost");

            Settings.Alter<RabbitMqOptions>(settings =>
            {
                settings.Connections.Add("messages3", "host=localhost");
                settings.Connections.Add("replies", "host=localhost");
            });

            Transports.ListenForMessagesFrom("rabbitmq://messages3/queue/messages3");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("rabbitmq://messages3/queue/messages3");

            Include<MessageTrackingExtension>();
        }
    }

    public class ColorHandler
    {
        public void Handle(ColorChosen message, ColorHistory history, Envelope envelope, MessageTracker tracker)
        {
            history.Name = message.Name;
            history.Envelope = envelope;
            tracker.Record(message, envelope);
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

    public class TracksMessage<T>
    {
        public void Handle(T message, MessageTracker tracker, Envelope envelope)
        {
            tracker.Record(message, envelope);
        }
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
}
