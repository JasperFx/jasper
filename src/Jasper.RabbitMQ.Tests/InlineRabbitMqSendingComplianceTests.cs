using System;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class InlineSender : JasperOptions
    {
        public static int Count = 0;

        public InlineSender()
        {
            QueueName = $"compliance{++Count}";
            var listener = $"listener{Count}";

            this.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
                x.DeclareQueue(QueueName);
                x.DeclareQueue(listener);
                x.AutoProvision = true;
                x.AutoPurgeOnStartup = true;
            });

            this.ListenToRabbitQueue(listener).UseForReplies().ProcessInline();

            PublishAllMessages().ToRabbit(QueueName).SendInline();
        }

        public string QueueName { get; set; }
    }

    public class InlineReceiver : JasperOptions
    {
        public InlineReceiver(string queueName)
        {
            this.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
            });

            this.ListenToRabbitQueue(queueName).ProcessInline();


        }
    }


    public class InlineRabbitMqSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {

        public InlineRabbitMqSendingFixture() : base($"rabbitmq://queue/{RabbitTesting.NextQueueName()}".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            var sender = new InlineSender();
            OutboundAddress = $"rabbitmq://routing/{sender.QueueName}".ToUri();

            await SenderIs(sender);

            var receiver = new InlineReceiver(sender.QueueName);

            await ReceiverIs(receiver);
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

    }


    [Collection("acceptance")]
    public class InlineRabbitMqSendingComplianceTests : SendingCompliance<InlineRabbitMqSendingFixture>
    {

    }

}
