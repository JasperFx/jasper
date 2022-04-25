using System;
using System.Linq;
using Jasper.RabbitMQ.Internal;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.RabbitMQ.Tests
{
    public class when_adding_bindings
    {
        private readonly RabbitMqTransport theTransport = new RabbitMqTransport();
        private Binding theBinding;

        public when_adding_bindings()
        {
            theBinding = new Binding
            {
                BindingKey = "key3",
                ExchangeName = "exchange3",
                QueueName = "queue3"
            };

            theTransport.BindExchange("exchange3").ToQueue("queue3", "key3");
        }

        [Fact]
        public void should_add_the_binding()
        {
            theTransport.Bindings.Single()
                .ShouldBe(theBinding);
        }

        [Fact]
        public void should_declare_the_exchange()
        {
            theTransport.Exchanges.Has(theBinding.ExchangeName);
        }

        [Fact]
        public void should_declare_the_queue()
        {
            theTransport.Queues.Has(theBinding.QueueName);
        }

    }
}
