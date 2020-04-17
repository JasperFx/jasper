using System;
using System.Threading.Tasks;
using Baseline.Dates;
using Confluent.Kafka;
using Jasper.Tracking;
using Jasper.Util;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.ConfluentKafka.Tests
{
    [Obsolete("try to replace with compliance tests")]
    public class end_to_end
    {
        private static string KafkaServer = "b-1.jj-test.y7lv7k.c5.kafka.us-east-1.amazonaws.com:9094,b-2.jj-test.y7lv7k.c5.kafka.us-east-1.amazonaws.com:9094";
        private static ProducerConfig ProducerConfig = new ProducerConfig
        {
            BootstrapServers = KafkaServer,
            SecurityProtocol = SecurityProtocol.Ssl
        };

        private static ConsumerConfig ConsumerConfig = new ConsumerConfig
        {
            BootstrapServers = KafkaServer,
            SecurityProtocol = SecurityProtocol.Ssl,
            GroupId = nameof(end_to_end),
        };

        public class Sender : JasperOptions
        {

            public Sender()
            {
                Endpoints.ConfigureKafka();

            }

            public string QueueName { get; set; }
        }

        public class Receiver : JasperOptions
        {
            public Receiver(string queueName)
            {
                Endpoints.ConfigureKafka();
            }
        }


        public class KafkaSendingComplianceTests : SendingCompliance
        {
            public KafkaSendingComplianceTests() : base($"kafka://topic/messages".ToUri())
            {
                var sender = new Sender();

                SenderIs(sender);

                var receiver = new Receiver(sender.QueueName);

                ReceiverIs(receiver);
            }
        }





        // SAMPLE: can_stop_and_start_ASB
        [Fact]
        public async Task can_stop_and_start()
        {
            using (var host = JasperHost.For<KafkaUsingApp>())
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
        public class KafkaUsingApp : JasperOptions
        {
            public KafkaUsingApp()
            {
                Endpoints.ConfigureKafka();
                Endpoints.ListenToKafkaTopic<string, ColorChosen>("messages", ConsumerConfig);
                Endpoints.PublishAllMessages().ToKafkaTopic<string, ColorChosen>("messages", ProducerConfig);

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
}
