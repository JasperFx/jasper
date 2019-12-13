using System;
using System.Collections.Generic;
using Baseline;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class Binding
    {
        public string BindingKey { get; set; }
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }

        public IDictionary<string, object> Arguments { get; set; } = new Dictionary<string, object>();

        internal void Declare(IModel channel)
        {
            channel.QueueBind(QueueName, ExchangeName, BindingKey, Arguments);
        }

        public void Teardown(IModel channel)
        {
            channel.QueueUnbind(QueueName, ExchangeName, BindingKey, Arguments);
        }

        internal void AssertValid()
        {
            if (BindingKey.IsEmpty() || QueueName.IsEmpty() || ExchangeName.IsEmpty())
            {
                throw new InvalidOperationException($"{nameof(BindingKey)} properties {nameof(BindingKey)}, {nameof(QueueName)}, and {nameof(ExchangeName)} are all required for this operation");
            }
        }
    }
}