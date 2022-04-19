using System;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class InlineRabbitMqSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {

        public InlineRabbitMqSendingFixture() : base($"rabbitmq://queue/{RabbitTesting.NextQueueName()}".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            var queueName = RabbitTesting.NextQueueName();
            OutboundAddress = $"rabbitmq://routing/{queueName}".ToUri();

            await SenderIs(opts =>
            {
                var listener = RabbitTesting.NextQueueName();

                opts.UseRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(queueName);
                    x.DeclareQueue(listener);
                    x.AutoProvision = true;
                    x.AutoPurgeOnStartup = true;
                });

                opts.ListenToRabbitQueue(listener).UseForReplies().ProcessInline();

                opts.PublishAllMessages().ToRabbit(queueName).SendInline();
            });

            await ReceiverIs(opts =>
            {
                opts.UseRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                });

                opts.ListenToRabbitQueue(queueName).ProcessInline();
            });
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
