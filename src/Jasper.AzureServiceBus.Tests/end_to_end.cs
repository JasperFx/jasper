using System;
using System.Threading.Tasks;
using Baseline.Dates;
using IntegrationTests;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Persistence;
using Jasper.Persistence.Marten;
using Jasper.Tracking;
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
            using (var host = JasperHost.For<ASBUsingApp>())
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
        public async Task schedule_send_message_to_and_receive_through_asb_with_durable_transport_option()
        {
            var publisher = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.PublishAllMessages().ToAzureServiceBusQueue("messages").Durably();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "sender";
                });

                _.Extensions.UseMessageTrackingTestingSupport();
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.ListenToAzureServiceBusQueue("messages");

                _.Extensions.UseMessageTrackingTestingSupport();
                _.Services.AddSingleton<ColorHistory>();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "receiver";
                });
            });

            receiver.RebuildMessageStorage();


            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .Timeout(15.Seconds())
                    .ExecuteAndWait(c => c.ScheduleSend(new ColorChosen {Name = "Orange"}, 5.Seconds()));

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
                await runtime
                    .TrackActivity()
                    .IncludeExternalTransports()
                    .SendMessageAndWait(new ColorChosen {Name = "Red"});

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_asb_with_durable_transport_option()
        {

            var publisher = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.PublishAllMessages().ToAzureServiceBusQueue("messages").Durably();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "sender";
                });

                _.Extensions.UseMessageTrackingTestingSupport();
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.ListenToAzureServiceBusQueue("messages").Durably();

                _.Services.AddSingleton<ColorHistory>();
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "receiver";
                });
            });

            receiver.RebuildMessageStorage();


            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .Timeout(10.Seconds())
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
        public async Task send_message_to_and_receive_through_asb_with_named_topic()
        {
            var uri = "azureservicebus://jasper/topic/special";

            var publisher = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.PublishAllMessages().ToAzureServiceBusTopic("special").Durably();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "sender";
                });

                _.Extensions.UseMessageTrackingTestingSupport();
            });

            publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.ListenToAzureServiceBusTopic("special", "receiver");

                _.Services.AddSingleton<ColorHistory>();
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "receiver";
                });
            });

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

    }


    public class ASBUsingApp : JasperOptions
    {
        public ASBUsingApp()
        {
            Endpoints.ListenToAzureServiceBusQueue("messages");
            Endpoints.PublishAllMessages().ToAzureServiceBusQueue("messages");
            Endpoints.ConfigureAzureServiceBus(end_to_end.ConnectionString);

            Services.AddSingleton<ColorHistory>();

            Extensions.UseMessageTrackingTestingSupport();
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

    public class TracksMessage<T>
    {
        public void Handle(T message)
        {
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
