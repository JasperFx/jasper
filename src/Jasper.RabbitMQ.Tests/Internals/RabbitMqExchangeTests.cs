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
        public void exchange_declare()
        {
            var channel = Substitute.For<IModel>();
            var exchange = new RabbitMqExchange("foo")
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
            var exchange = new RabbitMqExchange("foo")
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
