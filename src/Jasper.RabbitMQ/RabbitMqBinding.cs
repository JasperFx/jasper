using System;
using System.Collections.Generic;
using Baseline;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
{
    public class RabbitMqBinding
    {
        public RabbitMqBinding(string exchangeName, RabbitMqQueue queue, string? bindingKey = null)
        {
            ExchangeName = exchangeName ?? throw new ArgumentNullException(nameof(exchangeName));
            Queue = queue ?? throw new ArgumentNullException(nameof(queue));
            BindingKey = bindingKey ?? $"{ExchangeName}_{Queue.Name}";
        }

        public string BindingKey { get; }
        public RabbitMqQueue Queue { get; }
        public string ExchangeName { get; }

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        internal void Declare(IModel channel)
        {
            Queue.Declare(channel);
            channel.QueueBind(Queue.Name, ExchangeName, BindingKey, Arguments);
        }

        public void Teardown(IModel channel)
        {
            channel.QueueUnbind(Queue.Name, ExchangeName, BindingKey, Arguments);
        }

        protected bool Equals(RabbitMqBinding other)
        {
            return BindingKey == other.BindingKey && Queue == other.Queue && ExchangeName == other.ExchangeName;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((RabbitMqBinding)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BindingKey, Queue, ExchangeName);
        }

        public override string ToString()
        {
            return
                $"{nameof(BindingKey)}: {BindingKey}, {nameof(Queue)}: {Queue}, {nameof(ExchangeName)}: {ExchangeName}";
        }
    }
}
