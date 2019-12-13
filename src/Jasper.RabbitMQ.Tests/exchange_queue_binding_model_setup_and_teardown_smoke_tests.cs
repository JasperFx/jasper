using System;
using System.Threading;
using Jasper.RabbitMQ.Internal;
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

            theTransport.DeclareBinding(new Binding
            {
                ExchangeName = "direct1",
                QueueName = "queue1",
                BindingKey = "key1"
            });

            theTransport.DeclareBinding(new Binding
            {
                ExchangeName = "fan1",
                QueueName = "queue2",
                BindingKey = "key2"
            });


        }

        [Fact]
        public void declare_all()
        {
            theTransport.DeclareAll();
        }

        [Fact]
        public void purge_from_all()
        {
            theTransport.DeclareAll();
            theTransport.PurgeAllQueues();
        }

        [Fact]
        public void delete_all()
        {
            theTransport.DeclareAll();
            theTransport.TeardownAll();
        }
    }
}
