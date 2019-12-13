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

            theTransport.DeclareBinding(theBinding);
        }

        [Fact]
        public void should_add_the_binding()
        {
            theTransport.Bindings.Single()
                .ShouldBeSameAs(theBinding);
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

        [Fact]
        public void sad_path_binding_key_missing()
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                theTransport.DeclareBinding(new Binding
                {
                    QueueName = "a",
                    ExchangeName = "b"
                });
            });
        }

        [Fact]
        public void sad_path_queue_name_missing()
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                theTransport.DeclareBinding(new Binding
                {
                    BindingKey = "a",
                    ExchangeName = "b"
                });
            });
        }

        [Fact]
        public void sad_path_exchange_name_missing()
        {
            Should.Throw<InvalidOperationException>(() =>
            {
                theTransport.DeclareBinding(new Binding
                {
                    BindingKey = "a",
                    QueueName = "b"
                });
            });
        }


    }
}
