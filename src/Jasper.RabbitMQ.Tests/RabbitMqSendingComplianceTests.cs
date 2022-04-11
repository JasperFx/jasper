using System;
using System.Threading.Tasks;
using Jasper.Util;
using TestingSupport.Compliance;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{

    public class RabbitMqSendingFixture : SendingComplianceFixture, IAsyncLifetime
    {
        public RabbitMqSendingFixture() : base($"rabbitmq://queue/{RabbitTesting.NextQueueName()}".ToUri())
        {

        }

        public async Task InitializeAsync()
        {
            var queueName = RabbitTesting.NextQueueName();
            OutboundAddress = $"rabbitmq://queue/{queueName}".ToUri();

            await SenderIs(opts =>
            {

                var listener = $"listener{RabbitTesting.Number}";

                opts.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue(queueName);
                    x.DeclareQueue(listener);
                    x.AutoProvision = true;
                    x.AutoPurgeOnStartup = true;
                });

                opts.ListenToRabbitQueue(listener).UseForReplies();
            });

            await ReceiverIs(opts =>
            {
                opts.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                });

                opts.ListenToRabbitQueue(queueName);
            });
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }

    [Collection("acceptance")]
    public class RabbitMqSendingComplianceTests : SendingCompliance<RabbitMqSendingFixture>
    {

    }
}
