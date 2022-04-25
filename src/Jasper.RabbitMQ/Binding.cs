using System;
using System.Collections.Generic;
using Baseline;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ
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

        protected bool Equals(Binding other)
        {
            return BindingKey == other.BindingKey && QueueName == other.QueueName && ExchangeName == other.ExchangeName;
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

            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((Binding)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BindingKey, QueueName, ExchangeName);
        }

        public override string ToString()
        {
            return $"{nameof(BindingKey)}: {BindingKey}, {nameof(QueueName)}: {QueueName}, {nameof(ExchangeName)}: {ExchangeName}";
        }
    }
}
