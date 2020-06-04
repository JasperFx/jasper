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


    public class InlineRabbitMqSendingComplianceTests : SendingCompliance
    {
        public InlineRabbitMqSendingComplianceTests() : base($"rabbitmq://queue/compliance".ToUri())
        {
            var sender = new InlineSender();
            theOutboundAddress = $"rabbitmq://routing/{sender.QueueName}".ToUri();

            SenderIs(sender);



            theSender.TryPurgeAllRabbitMqQueues();

            var receiver = new InlineReceiver(sender.QueueName);

            ReceiverIs(receiver);
        }
    }

}
