using System;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Jasper.RabbitMQ.Internal;
using Oakton.Resources;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class exchange_queue_binding_model_setup_and_teardown_smoke_tests
    {
        private readonly RabbitMqTransport theTransport = new RabbitMqTransport();

        public exchange_queue_binding_model_setup_and_teardown_smoke_tests()
        {
            theTransport.ConnectionFactory.HostName = "localhost";

            theTransport.DeclareExchange("direct1", exchange =>
            {
                exchange.IsDurable = true;
                exchange.ExchangeType = ExchangeType.Direct;
            });

            theTransport.DeclareExchange("fan1", exchange =>
            {
                exchange.ExchangeType = ExchangeType.Fanout;
            });

            theTransport.DeclareQueue("queue1");
            theTransport.DeclareQueue("queue2");

            theTransport
                .BindExchange("direct1")
                .ToQueue("queue1", "key1");

            theTransport
                .BindExchange("fan1")
                .ToQueue("queue2", "key2");
        }

        [Fact]
        public async Task resource_setup()
        {
            await theTransport.As<IStatefulResource>().Setup(CancellationToken.None);
        }

        [Fact]
        public async Task clear_state_as_resource()
        {
            await theTransport.As<IStatefulResource>().Setup(CancellationToken.None);
            await theTransport.As<IStatefulResource>().ClearState(CancellationToken.None);
        }

        [Fact]
        public async Task delete_all()
        {
            await theTransport.As<IStatefulResource>().Setup(CancellationToken.None);
            await theTransport.As<IStatefulResource>().Teardown(CancellationToken.None);
        }
    }
}
