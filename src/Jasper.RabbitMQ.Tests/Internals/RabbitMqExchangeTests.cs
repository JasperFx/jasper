using Baseline.Reflection;
using Jasper.RabbitMQ.Internal;
using NSubstitute;
using RabbitMQ.Client;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Tests.Internals
{
    public class configuration_model_specs
    {
        [Fact]
        public void defaults()
        {
            var exchange = new RabbitMqExchange("foo", new RabbitMqTransport());
            exchange.Name.ShouldBe("foo");
            exchange.ExchangeType.ShouldBe(ExchangeType.Fanout);
            exchange.AutoDelete.ShouldBeFalse();
            exchange.IsDurable.ShouldBeTrue();
        }

        [Fact]
        public void exchange_declare()
        {
            var channel = Substitute.For<IModel>();
            var exchange = new RabbitMqExchange("foo", new RabbitMqTransport())
            {
                ExchangeType = ExchangeType.Fanout,
                AutoDelete = true,
                IsDurable = false
            };

            exchange.Declare(channel);

            channel.Received().ExchangeDeclare("foo", "fanout", false, true, exchange.Arguments);

            exchange.HasDeclared.ShouldBeTrue();
        }

        [Fact]
        public void already_latched()
        {
            var channel = Substitute.For<IModel>();
            var exchange = new RabbitMqExchange("foo", new RabbitMqTransport())
            {
                ExchangeType = ExchangeType.Fanout,
                AutoDelete = true,
                IsDurable = false
            };

            // cheating here.
            var prop = ReflectionHelper.GetProperty<RabbitMqExchange>(x => x.HasDeclared);
            prop.SetValue(exchange, true);

            exchange.Declare(channel);


            channel.DidNotReceiveWithAnyArgs();

        }




    }
}
