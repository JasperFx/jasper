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
using TestingSupport.Compliance;
using Weasel.Postgresql;
using Xunit;

namespace Jasper.AzureServiceBus.Tests
{
    public class end_to_end
    {
        // TODO -- make this puppy be pulled from an environment variable? Something ignored?
        public const string ConnectionString =
            "Endpoint=sb://jaspertest.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=tYfuj6uX/L2kolyKi+dc7Jztu45vHVp4wf3W+YBoXHc=";




        public class Sender : JasperOptions
        {

            public Sender()
            {

                Endpoints.ConfigureAzureServiceBus(ConnectionString);

                Endpoints.ListenToAzureServiceBusQueue("replies").UseForReplies();

            }

            public string QueueName { get; set; }
        }

        public class Receiver : JasperOptions
        {
            public Receiver(string queueName)
            {
                Endpoints.ConfigureAzureServiceBus(ConnectionString);

                Endpoints.ListenToAzureServiceBusQueue("messages");


            }
        }

        public class AzureServiceBusSendingFixture : SendingComplianceFixture, IAsyncLifetime
        {
            public AzureServiceBusSendingFixture() : base($"asb://queue/messages".ToUri())
            {

            }

            public async Task InitializeAsync()
            {
                var sender = new Sender();

                await SenderIs(sender);

                var receiver = new Receiver(sender.QueueName);

                await ReceiverIs(receiver);
            }

            public Task DisposeAsync()
            {
                return Task.CompletedTask;
            }
        }


        public class AzureServiceBusSendingComplianceTests : SendingCompliance<AzureServiceBusSendingFixture>
        {
            public AzureServiceBusSendingComplianceTests(AzureServiceBusSendingFixture fixture) : base(fixture)
            {
            }
        }





        // SAMPLE: can_stop_and_start_ASB
        [Fact]
        public async Task can_stop_and_start()
        {
            using (var host = JasperHost.For<ASBUsingApp>())
            {
                await host
                    // The TrackActivity() method starts a Fluent Interface
                    // that gives you fine-grained control over the
                    // message tracking
                    .TrackActivity()
                    .Timeout(30.Seconds())
                    // Include the external transports in the determination
                    // of "completion"
                    .IncludeExternalTransports()
                    .SendMessageAndWait(new ColorChosen {Name = "Red"});

                var colors = host.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }
        // ENDSAMPLE

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

            await publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Handlers.IncludeType<ColorHandler>();

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

            await receiver.RebuildMessageStorage();


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

            await publisher.RebuildMessageStorage();

            var receiver = JasperHost.For(_ =>
            {
                _.Endpoints.ConfigureAzureServiceBus(ConnectionString);
                _.Endpoints.ListenToAzureServiceBusQueue("messages").DurablyPersistedLocally();

                _.Services.AddSingleton<ColorHistory>();
                _.Extensions.UseMessageTrackingTestingSupport();

                _.Extensions.UseMarten(opts =>
                {
                    opts.Connection(Servers.PostgresConnectionString);
                    opts.AutoCreateSchemaObjects = AutoCreate.All;
                    opts.DatabaseSchemaName = "receiver";
                });

                _.Handlers.IncludeType<ColorHandler>();
            });

            await receiver.RebuildMessageStorage();


            try
            {
                await publisher
                    .TrackActivity()
                    .AlsoTrack(receiver)
                    .Timeout(30.Seconds())
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

            await publisher.RebuildMessageStorage();

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

                _.Handlers.IncludeType<ColorHandler>();

            });

            await receiver.RebuildMessageStorage();


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

            Handlers.IncludeType<ColorHandler>();

            Services.AddSingleton<ColorHistory>();

            Extensions.UseMessageTrackingTestingSupport();
        }

        public override void Configure(IHostEnvironment hosting, IConfiguration config)
        {
            //Endpoints.ConfigureAzureServiceBus(config.GetValue<string>("AzureServiceBusConnectionString"));
        }
    }


}
