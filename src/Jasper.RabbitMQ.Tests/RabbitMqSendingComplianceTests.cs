using System;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{

    public class Sender : JasperOptions
    {
        public Sender()
        {
            QueueName = RabbitTesting.NextQueueName();
            var listener = $"listener{RabbitTesting.Number}";

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
        public RabbitMqSendingFixture() : base($"rabbitmq://queue/{RabbitTesting.NextQueueName()}".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            var sender = new Sender();
            OutboundAddress = $"rabbitmq://queue/{sender.QueueName}".ToUri();

            await SenderIs(sender);

            var receiver = new Receiver(sender.QueueName);

            await ReceiverIs(receiver);

            Sender.TryPurgeAllRabbitMqQueues();
            Receiver.TryPurgeAllRabbitMqQueues();
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

    [Collection("acceptance")]
    public class RabbitMqSendingComplianceTests : SendingCompliance<RabbitMqSendingFixture>
    {

    }
}
