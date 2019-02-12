using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class end_to_end
    {
        // TODO -- make this puppy be pulled from an environment variable? Something ignored?
        public const string ConnectionString = "Endpoint=sb://jaspertest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=/SnqkkF1Vx0n8GjbzWFmPHeW5vmxCP8dJ7OSZrR9g4k=";


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
        public async Task schedule_send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var uri = "azureservicebus://jasper/durable/queue/messages";

            var publisher = JasperHost.For(_ =>
            {
                _.Settings.AddAzureServiceBusConnection("jasper", ConnectionString);

                _.HttpRoutes.DisableConventionalDiscovery();

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

                _.HttpRoutes.DisableConventionalDiscovery();

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











    }


    public class ASBUsingApp : JasperWithAzureServiceBusRegistry
    {
        public ASBUsingApp()
        {

            HttpRoutes.DisableConventionalDiscovery();

            Transports.ListenForMessagesFrom("azureservicebus://jasper/queue/messages");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("azureservicebus://jasper/queue/messages");

            Include<MessageTrackingExtension>();
        }

        protected override void Configure(IHostingEnvironment contextHostingEnvironment, IConfiguration configuration,
            AzureServiceBusSettings settings)
        {
            settings.Connections.Add("jasper", end_to_end.ConnectionString);
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
}
