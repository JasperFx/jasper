using System;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.Util;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class end_to_end
    {
        // TODO -- make this puppy be pulled from an environment variable? Something ignored?
        public const string ConnectionString =
            "Endpoint=sb://jaspertest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=tYfuj6uX/L2kolyKi+dc7Jztu45vHVp4wf3W+YBoXHc=";


        [Fact]
        public async Task can_stop_and_start()
        {
            using (var runtime = JasperHost.For<ASBUsingApp>())
            {
                var root = runtime.Get<IMessagingRoot>();
                root.ListeningStatus = ListeningStatus.TooBusy;
                root.ListeningStatus = ListeningStatus.Accepting;


                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }

        [Fact]
        public async Task schedule_send_message_to_and_receive_through_asb_with_durable_transport_option()
        {
            var uri = "azureservicebus://jasper/durable/queue/messages";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);


                _.Publish.AllMessagesTo(uri);

                _.Include<MartenBackedPersistence>();

                _.Settings.ConfigureMarten(x =>
                {
                    x.Connection(Servers.PostgresConnectionString);
                    x.AutoCreateSchemaObjects = AutoCreate.All;
                });
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);


                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

                _.Include<MartenBackedPersistence>();

                _.Settings.MartenConnectionStringIs(Servers.PostgresConnectionString);
            });

            receiver.RebuildMessageStorage();

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Messaging.ScheduleSend(new ColorChosen {Name = "Orange"}, 5.Seconds());

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
        public async Task send_message_to_and_receive_through_asb()
        {
            using (var runtime = JasperHost.For<ASBUsingApp>())
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_asb_with_durable_transport_option()
        {
            var uri = "azureservicebus://jasper/durable/queue/messages";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);
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
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);

                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

                _.Include<MartenBackedPersistence>();

                _.Settings.MartenConnectionStringIs(Servers.PostgresConnectionString);
            });

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Messaging.Send(new ColorChosen {Name = "Orange"});

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
        public async Task send_message_to_and_receive_through_asb_with_named_topic()
        {
            var uri = "azureservicebus://jasper/topic/special";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);
                _.Publish.AllMessagesTo(uri);
                _.Handlers.DisableConventionalDiscovery();
            });

            var receiver = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);

                _.Transports.ListenForMessagesFrom("azureservicebus://jasper/subscription/receiver/topic/special");
                _.Services.AddSingleton<MessageTracker>();

                _.Handlers.DisableConventionalDiscovery().IncludeType<TracksMessage<SpecialTopic>>();
            });

            var wait = receiver.Get<MessageTracker>().WaitFor<SpecialTopic>();

            try
            {
                var message = new SpecialTopic();
                await publisher.Messaging.Send(message);

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
        public async Task send_message_to_and_receive_through_asb_with_wildcard_topics()
        {
            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);
                _.Publish.AllMessagesTo("azureservicebus://jasper/topic/*");
                _.Handlers.DisableConventionalDiscovery();
            });

            var receiver1 = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);

                _.Transports.ListenForMessagesFrom("azureservicebus://jasper/subscription/receiver1/topic/*");
                _.Services.AddSingleton<MessageTracker>();

                _.Handlers.DisableConventionalDiscovery()
                    .IncludeType<TracksMessage<TopicA>>()
                    .IncludeType<TracksMessage<TopicB>>();
            });

            var receiver2 = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);

                _.Transports.ListenForMessagesFrom("azureservicebus://jasper/subscription/receiver2/topic/*");
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

                await publisher.Messaging.Send(topicA);
                await publisher.Messaging.Send(topicB);
                await publisher.Messaging.Send(topicC);

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


    public class ASBUsingApp : JasperWithAzureServiceBusRegistry
    {
        public ASBUsingApp()
        {
            Transports.ListenForMessagesFrom("azureservicebus://jasper/queue/messages");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("azureservicebus://jasper/queue/messages");

            Include<MessageTrackingExtension>();
        }

        protected override void Configure(IHostEnvironment contextHostingEnvironment, IConfiguration configuration,
            AzureServiceBusOptions options)
        {
            options.Connections.Add("jasper", end_to_end.ConnectionString);
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
