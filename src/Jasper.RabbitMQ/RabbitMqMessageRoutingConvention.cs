using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Util;

namespace Jasper.RabbitMQ
{
    public class RabbitMappingContext
    {
        public Type MessageType { get; }
        public RabbitMqTransport Transport { get; }
        public IJasperRuntime Runtime { get; }
        public RabbitMqEndpoint Endpoint { get; }

        public RabbitMappingContext(Type messageType, RabbitMqTransport transport, IJasperRuntime runtime, RabbitMqEndpoint endpoint)
        {
            MessageType = messageType;
            Transport = transport;
            Runtime = runtime;
            Endpoint = endpoint;
        }
    }

    /// <summary>
    /// Conventional message routing for Rabbit MQ. By default, sends messages to an
    /// exchange named after the MessageTypeName bound to a queue of the same name.
    /// </summary>
    public class RabbitMqMessageRoutingConvention : IMessageRoutingConvention
    {
        public void DiscoverListeners(IJasperRuntime runtime, IReadOnlyList<Type> handledMessageTypes)
        {
            var transport = runtime.Options.RabbitMqTransport();

            foreach (var messageType in handledMessageTypes)
            {
                // Can be null, so bail out if there's no queue
                var queueName = _queueNameForListener(messageType);
                if (queueName.IsEmpty()) return;

                var endpoint = transport.EndpointForQueue(queueName);
                endpoint.Mode = Mode;
                var queue = transport.Queues[queueName];

                var context = new RabbitMappingContext(messageType, transport, runtime, endpoint);

                _configureListener(queue, context);
            }
        }

        public IEnumerable<Endpoint> DiscoverSenders(Type messageType, IJasperRuntime runtime)
        {
            var transport = runtime.Options.RabbitMqTransport();

            // HAVE THIS BUILD THE EXCHANGE, and alternatively do all the bindings. Return the sending endpoint!
            var exchangeName = _exchangeNameForSending(messageType);
            if (exchangeName.IsEmpty()) yield break;
            var exchange = transport.Exchanges[exchangeName];

            var endpoint = transport.EndpointForExchange(exchangeName);
            endpoint.Mode = Mode;
            _configureSending(exchange, new RabbitMappingContext(messageType, transport, runtime, endpoint));

            // This will start up the sending agent
            var sendingAgent = runtime.GetOrBuildSendingAgent(endpoint.Uri);
            yield return sendingAgent.Endpoint;
        }

        internal EndpointMode Mode { get; set; } = EndpointMode.BufferedInMemory;

        private Func<Type, string?> _queueNameForListener = t => t.ToMessageTypeName();
        private Action<RabbitMqQueue, RabbitMappingContext> _configureListener = (_, _) => { };
        private Func<Type, string?> _exchangeNameForSending = t => t.ToMessageTypeName();

        private Action<RabbitMqExchange, RabbitMappingContext> _configureSending = (exchange, context) =>
        {
            exchange.BindQueue(exchange.Name);
        };

        /// <summary>
        /// Override the convention for determining the exchange name for outgoing messages of the message type.
        /// Returning null or empty is interpreted as "don't publish this message type". Default is the MessageTypeName
        /// </summary>
        /// <param name="nameSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMqMessageRoutingConvention ExchangeNameForSending(Func<Type, string?> nameSource)
        {
            _exchangeNameForSending = nameSource ?? throw new ArgumentNullException(nameof(nameSource));
            return this;
        }

        /// <summary>
        /// Override the convention for determining the queue name for receiving incoming messages of the message type.
        /// Returning null or empty is interpreted as "don't create a new queue for this message type". Default is the MessageTypeName
        /// </summary>
        /// <param name="nameSource"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RabbitMqMessageRoutingConvention QueueNameForListener(Func<Type, string?> nameSource)
        {
            _queueNameForListener = nameSource ?? throw new ArgumentNullException(nameof(nameSource));
            return this;
        }

        /// <summary>
        /// Override the Rabbit MQ and Jasper configuration for new listening endpoints created by message type.
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public RabbitMqMessageRoutingConvention ConfigureListener(Action<RabbitMqQueue, RabbitMappingContext> configure)
        {
            _configureListener = configure ?? throw new ArgumentNullException(nameof(configure));
            return this;
        }

        /// <summary>
        /// Override the Rabbit MQ and Jasper configuration for sending endpoints, exchanges, and queue bindings
        /// for a new sending endpoint
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public RabbitMqMessageRoutingConvention ConfigureSending(Action<RabbitMqExchange, RabbitMappingContext> configure)
        {
            _configureSending = configure ?? throw new ArgumentNullException(nameof(configure));
            return this;
        }

        /// <summary>
        /// All listening and sending endpoints will be durable with persistent Inbox or Outbox
        /// mechanics
        /// </summary>
        /// <returns></returns>
        public RabbitMqMessageRoutingConvention InboxedListenersAndOutboxedSenders()
        {
            Mode = EndpointMode.Durable;
            return this;
        }

        /// <summary>
        /// All listening and sending endpoints will use the inline mode
        /// </summary>
        /// <returns></returns>
        public RabbitMqMessageRoutingConvention InlineListenersAndSenders()
        {
            Mode = EndpointMode.Inline;
            return this;
        }

        /// <summary>
        /// All listening and sending endpoints will use buffered mechanics. This is the default
        /// </summary>
        /// <returns></returns>
        public RabbitMqMessageRoutingConvention BufferedListenersAndSenders()
        {
            Mode = EndpointMode.BufferedInMemory;
            return this;
        }
    }
}
