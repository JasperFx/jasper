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
        public async Task send_message_to_and_receive_through_rabbitmq()
        {
            using (var host = JasperHost.For<RabbitMqUsingApp>())
            {
                await host
                    .TrackActivity()
                    .IncludeExternalTransports()
                    .SendMessageAndWait(new ColorChosen {Name = "Red"});

                var colors = host.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var uri = "rabbitmq://default/messages2/durable";

            var publisher = JasperHost.For(_ =>
            {
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("messages2");
                    x.AutoProvision = true;
                });

                _.Endpoints.PublishAllMessages().To(uri);

                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "sender";
                });

            });

            var receiver = JasperHost.For(_ =>
            {
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("messages2");
                    x.AutoProvision = true;
                });

                _.Endpoints.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();

                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "receiver";
                });
            });

            publisher.RebuildMessageStorage();
            receiver.RebuildMessageStorage();


            try
            {

                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .SendMessageAndWait(new ColorChosen {Name = "Orange"});


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
            var queueName = "messages5";


            var publisher = JasperHost.For(_ =>
            {
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(queueName);
                    x.DeclareExchange("exchange1");
                    x.DeclareBinding(new Binding
                    {
                        ExchangeName = "exchange1",
                        BindingKey = "key2",
                        QueueName =  queueName
                    });

                    x.AutoProvision = true;
                });

                _.Endpoints.PublishAllMessages().To("rabbitmq://exchange1/key2");

            });

            var receiver = JasperHost.For(_ =>
            {
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("messages5");
                    x.DeclareExchange("exchange1");
                    x.DeclareBinding(new Binding
                    {
                        ExchangeName = "exchange1",
                        BindingKey = "key2",
                        QueueName =  queueName
                    });

                    x.AutoProvision = true;
                });

                _.Services.AddSingleton<ColorHistory>();

                _.Endpoints.ListenForMessagesFrom($"rabbitmq://exchange1/{queueName}");
            });

            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .SendMessageAndWait(new ColorChosen {Name = "Orange"});

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
            var uri = "rabbitmq://default/messages11/durable";

            var publisher = JasperHost.For(_ =>
            {
                _.ServiceName = "Publisher";

                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("messages11");


                    x.AutoProvision = true;
                });


                _.Endpoints.PublishAllMessages().To(uri);



                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "rabbit_sender";
                });
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.ServiceName = "Receiver";

                _.Extensions.UseMessageTrackingTestingSupport();

                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("messages11");


                    x.AutoProvision = true;
                });



                _.Endpoints.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();

                _.Extensions.UseMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                    x.DatabaseSchemaName = "rabbit_receiver";
                });
            });

            receiver.RebuildMessageStorage();



            try
            {
                await publisher.ExecuteAndWait(c => c.ScheduleSend(new ColorChosen {Name = "Orange"}, 5.Seconds()));

                // Forcing the receiver to wait until something happens
                await receiver.ExecuteAndWait(c => Task.CompletedTask, 10000);

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
            throw new NotImplementedException("Redo");
//            var uri = "rabbitmq://localhost/queue/messages";
//
//            var publisher = JasperHost.For(_ =>
//            {
//                _.Settings.ConfigureRabbitMq(settings =>
//                {
//                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
//                });
//
//                _.Publish.AllMessagesTo(uri);
//
//            });
//
//            var receiver1 = JasperHost.For(_ =>
//            {
//                _.Settings.ConfigureRabbitMq(settings =>
//                {
//                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
//                });
//
//                _.Transports.ListenForMessagesFrom(uri);
//                _.Services.AddSingleton<ColorHistory>();
//                _.Services.AddSingleton<MessageTracker>();
//
//            });
//
//            var receiver2 = JasperHost.For(_ =>
//            {
//                _.Settings.ConfigureRabbitMq(settings =>
//                {
//                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
//                });
//
//                _.Transports.ListenForMessagesFrom(uri);
//                _.Services.AddSingleton<ColorHistory>();
//                _.Services.AddSingleton<MessageTracker>();
//
//            });
//
//            var receiver3 = JasperHost.For(_ =>
//            {
//                _.Settings.ConfigureRabbitMq(settings =>
//                {
//                    settings.Connections.Add("localhost", "host=localhost;ExchangeType=fanout;ExchangeName=north");
//                });
//
//                _.Transports.ListenForMessagesFrom(uri);
//                _.Services.AddSingleton<ColorHistory>();
//                _.Services.AddSingleton<MessageTracker>();
//
//            });
//
//            var wait1 = receiver1.Get<MessageTracker>().WaitFor<ColorChosen>();
//            var wait2 = receiver2.Get<MessageTracker>().WaitFor<ColorChosen>();
//            var wait3 = receiver3.Get<MessageTracker>().WaitFor<ColorChosen>();
//
//            try
//            {
//                await publisher.Send(new ColorChosen {Name = "Purple"});
//
//                await wait1;
//                //await wait2;
//                //await wait3;
//
//                receiver1.Get<ColorHistory>().Name.ShouldBe("Purple");
//                //receiver2.Get<ColorHistory>().Name.ShouldBe("Purple");
//                //receiver3.Get<ColorHistory>().Name.ShouldBe("Purple");
//            }
//            finally
//            {
//                publisher.Dispose();
//                receiver1.Dispose();
//                receiver2.Dispose();
//                receiver3.Dispose();
//            }
        }





        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_named_topic()
        {

            var uri = "rabbitmq://topics/special";

            var queueName = "messages4";

            var publisher = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareExchange("topics", ex => { ex.ExchangeType = ExchangeType.Topic; });
                    x.DeclareQueue("messages4");
                    x.DeclareBinding(new Binding
                    {
                        BindingKey = "special",
                        ExchangeName = "topics",
                        QueueName = queueName
                    });

                    x.AutoProvision = true;
                });

                _.Endpoints.PublishAllMessages().To("rabbitmq://topics/special");
                _.Handlers.DisableConventionalDiscovery();

                _.Extensions.UseMessageTrackingTestingSupport();

            });

            var receiver = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                });

                _.Endpoints.ListenForMessagesFrom($"rabbitmq://topics/{queueName}".ToUri());

                _.Extensions.UseMessageTrackingTestingSupport();

                _.Handlers.DisableConventionalDiscovery().IncludeType<SpecialTopicGuy>();

            });



            try
            {
                var message = new SpecialTopic();
                var session = await publisher.TrackActivity().AlsoTrack(receiver).SendMessageAndWait(message);


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


    public class RabbitMqUsingApp : JasperOptions
    {
        public RabbitMqUsingApp()
        {
            Extensions.UseMessageTrackingTestingSupport();

            Endpoints.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
                x.DeclareQueue("messages3");
                x.AutoProvision = true;
            });


            Endpoints.ListenForMessagesFrom("rabbitmq://default/messages3");

            Services.AddSingleton<ColorHistory>();

            Endpoints.PublishAllMessages().To("rabbitmq://default/messages3");

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
}
