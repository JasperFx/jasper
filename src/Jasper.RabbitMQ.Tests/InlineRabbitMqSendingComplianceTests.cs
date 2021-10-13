using System;
using Jasper.Util;
using TestingSupport.Compliance;

namespace Jasper.RabbitMQ.Tests
{
    public class InlineSender : JasperOptions
    {
        public static int Count = 0;

        public InlineSender()
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

            Endpoints.ListenToRabbitQueue(listener).UseForReplies().ProcessInline();

            Endpoints.PublishAllMessages().ToRabbit(QueueName).SendInline();
        }

        public string QueueName { get; set; }
    }

    public class InlineReceiver : JasperOptions
    {
        public InlineReceiver(string queueName)
        {
            Endpoints.ConfigureRabbitMq(x =>
            {
                x.ConnectionFactory.HostName = "localhost";
            });

            Endpoints.ListenToRabbitQueue(queueName).ProcessInline();


        }
    }

    public class InlineRabbitMqSendingFixture : SendingComplianceFixture
    {
        public InlineRabbitMqSendingFixture() : base($"rabbitmq://queue/compliance".ToUri())
        {
            var sender = new InlineSender();
            OutboundAddress = $"rabbitmq://routing/{sender.QueueName}".ToUri();

            SenderIs(sender);

            var receiver = new InlineReceiver(sender.QueueName);

            ReceiverIs(receiver);
        }

        public override void BeforeEach()
        {
            Sender.TryPurgeAllRabbitMqQueues();
        }
    }


    public class InlineRabbitMqSendingComplianceTests : SendingCompliance<InlineRabbitMqSendingFixture>
    {
        public InlineRabbitMqSendingComplianceTests(InlineRabbitMqSendingFixture fixture) : base(fixture)
        {
        }
    }

}
