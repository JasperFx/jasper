using System;

namespace Jasper.RabbitMQ.Internal
{
    public partial class RabbitMqTransport : IRabbitMqTransportExpression
    {
        IRabbitMqTransportExpression IRabbitMqTransportExpression.AutoProvision()
        {
            AutoProvision = true;
            return this;
        }

        IRabbitMqTransportExpression IRabbitMqTransportExpression.AutoPurgeOnStartup()
        {
            AutoPurgeAllQueues = true;
            return this;
        }


        public IRabbitMqTransportExpression DeclareExchange(string exchangeName,
            Action<RabbitMqExchange>? configure = null)
        {
            var exchange = Exchanges[exchangeName];
            configure?.Invoke(exchange);

            return this;
        }

        public IBindingExpression BindExchange(string exchangeName, ExchangeType exchangeType)
        {
            return BindExchange(exchangeName, e => e.ExchangeType = exchangeType);
        }

        public IRabbitMqTransportExpression DeclareQueue(string queueName, Action<RabbitMqQueue>? configure = null)
        {
            var queue = Queues[queueName];
            configure?.Invoke(queue);

            return this;
        }

        public IRabbitMqTransportExpression DeclareExchange(string exchangeName, ExchangeType exchangeType,
            bool isDurable = true, bool autoDelete = false)
        {
            return DeclareExchange(exchangeName, e =>
            {
                e.ExchangeType = exchangeType;
                e.IsDurable = isDurable;
                e.AutoDelete = autoDelete;
            });
        }
    }
}
