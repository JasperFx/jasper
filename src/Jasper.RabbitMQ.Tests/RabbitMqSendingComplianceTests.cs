using System;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{

    public class Sender : JasperOptions
    {
        public static int Count = 0;

        public Sender()
        {
            QueueName = $"compliance{++Count}";
            var listener = $"listener{Count}";

            Endpoints.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
                x.DeclareQueue(QueueName);
                x.DeclareQueue(listener);
                x.AutoProvision = true;
            });

            Endpoints.ListenToRabbitQueue(listener).UseForReplies();

        }

        public string QueueName { get; set; }
    }

    public class Receiver : JasperOptions
    {
        public Receiver(string queueName)
        {
            Endpoints.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
            });

            Endpoints.ListenToRabbitQueue(queueName);


        }
    }

    public class RabbitMqSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public RabbitMqSendingFixture() : base($"rabbitmq://queue/compliance".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            var sender = new Sender();
            OutboundAddress = $"rabbitmq://queue/{sender.QueueName}".ToUri();

            await SenderIs(sender);

            var receiver = new Receiver(sender.QueueName);

            await ReceiverIs(receiver);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public override void BeforeEach()
        {
            Sender.TryPurgeAllRabbitMqQueues();
        }
    }


    public class RabbitMqSendingComplianceTests : SendingCompliance<RabbitMqSendingFixture>
    {
        public RabbitMqSendingComplianceTests(RabbitMqSendingFixture fixture) : base(fixture)
        {
        }
    }
}
