using System;
using System.Collections.Generic;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqExchange
    {
        private readonly RabbitMqTransport _parent;

        public RabbitMqExchange(string name, RabbitMqTransport parent)
        {
            _parent = parent;
            Name = name;
            DeclaredName = name == TransportConstants.Default ? "" : Name;
        }

        public bool HasDeclared { get; private set; }

        public string Name { get; }

        public bool IsDurable { get; set; } = true;

        public string DeclaredName { get; }

        public ExchangeType ExchangeType { get; set; } = ExchangeType.Fanout;


        public bool AutoDelete { get; set; } = false;

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        internal void Declare(IModel channel)
        {
            if (DeclaredName == string.Empty)
            {
                return;
            }

            if (HasDeclared) return;

            var exchangeTypeName = ExchangeType.ToString().ToLower();
            channel.ExchangeDeclare(DeclaredName, exchangeTypeName, IsDurable, AutoDelete, Arguments);

            HasDeclared = true;
        }


        public void Teardown(IModel channel)
        {
            if (DeclaredName == string.Empty)
            {
                return;
            }

            channel.ExchangeDelete(DeclaredName);
        }


        /// <summary>
        /// Declare a Rabbit MQ binding with the supplied topic pattern to
        /// the queue
        /// </summary>
        /// <param name="topicPattern"></param>
        /// <param name="bindingName"></param>
        /// <exception cref="NotImplementedException"></exception>
        public TopicBinding BindTopic(string topicPattern)
        {
            return new TopicBinding(this, topicPattern);
        }

        public class TopicBinding
        {
            private readonly RabbitMqExchange _exchange;
            private readonly string _topicPattern;

            public TopicBinding(RabbitMqExchange exchange, string topicPattern)
            {
                _exchange = exchange;
                _topicPattern = topicPattern;
            }

            /// <summary>
            /// Create a binding of the topic pattern previously specified to a Rabbit Mq queue
            /// </summary>
            /// <param name="queueName">The name of the Rabbit Mq queue</param>
            /// <param name="configureQueue">Optionally configure </param>
            public void ToQueue(string queueName, Action<RabbitMqQueue>? configureQueue = null)
            {
                _exchange._parent.BindExchange(_exchange.Name).ToQueue(queueName, _topicPattern, configureQueue);
            }
        }
    }
}
