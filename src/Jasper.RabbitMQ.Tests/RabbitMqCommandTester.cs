using Jasper.RabbitMQ.CommandLine;
using Microsoft.Extensions.Hosting;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class RabbitMqCommandTester
    {
        private RabbitMqInput theInput;

        public RabbitMqCommandTester()
        {
            theInput = new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>()
            };

        }

        [Fact]
        public void can_declare()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();
        }

        [Fact]
        public void can_purge()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();

            new RabbitMqCommand().Execute(new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>(),
                Action = RabbitAction.purge
            }).ShouldBeTrue();
        }

        [Fact]
        public void can_delete()
        {
            theInput.Action = RabbitAction.setup;
            new RabbitMqCommand().Execute(theInput).ShouldBeTrue();

            new RabbitMqCommand().Execute(new RabbitMqInput
            {
                HostBuilder = Host.CreateDefaultBuilder().UseJasper<RabbitUsingApp>(),
                Action = RabbitAction.teardown
            }).ShouldBeTrue();
        }

        public class RabbitUsingApp : JasperOptions
        {
            public RabbitUsingApp()
            {
                Endpoints.ConfigureRabbitMq(x =>
                {
                    x.ConnectionFactory.HostName = "localhost";
                    x.DeclareQueue("queue15");
                    x.DeclareExchange("ex15", x => x.ExchangeType = ExchangeType.Fanout);
                    x.DeclareBinding(new Binding
                    {
                        BindingKey = "key1",
                        ExchangeName = "ex15",
                        QueueName = "queue15"
                    });
                });
            }
        }
    }
}
