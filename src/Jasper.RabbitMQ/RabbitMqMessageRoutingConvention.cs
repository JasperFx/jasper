using System;
using System.Collections.Generic;
using Baseline;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Util;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

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

    public class RabbitMqMessageRoutingConvention : IMessageRoutingConvention
    {
        public void DiscoverListeners(IJasperRuntime runtime, IReadOnlyList<Type> handledMessageTypes)
        {
            var transport = runtime.Options.RabbitMqTransport();

            foreach (var messageType in handledMessageTypes)
            {
                // Can be null, so bail out if there's no queue
                var queueName = QueueNameForListener(messageType);
                if (queueName.IsEmpty()) return;

                var endpoint = transport.EndpointForQueue(queueName);
                var queue = transport.Queues[queueName];

                var context = new RabbitMappingContext(messageType, transport, runtime, endpoint);

                ConfigureListener(queue, context);
            }
        }

        public IEnumerable<Endpoint> DiscoverSenders(Type messageType, IJasperRuntime runtime)
        {
            var transport = runtime.Options.RabbitMqTransport();

            // HAVE THIS BUILD THE EXCHANGE, and alternatively do all the bindings. Return the sending endpoint!
            var exchangeName = ExchangeNameForSending(messageType);
            if (exchangeName.IsEmpty()) yield break;
            var exchange = transport.Exchanges[exchangeName];

            var endpoint = transport.EndpointForExchange(exchangeName);
            ConfigureSending(exchange, new RabbitMappingContext(messageType, transport, runtime, endpoint));

            // This will start up the sending agent
            var sendingAgent = runtime.GetOrBuildSendingAgent(endpoint.Uri);
            yield return sendingAgent.Endpoint;
        }

        internal Func<Type, string?> QueueNameForListener = t => t.ToMessageTypeName();
        internal Action<RabbitMqQueue, RabbitMappingContext> ConfigureListener = (_, _) => { };
        internal Func<Type, string?> ExchangeNameForSending = t => t.ToMessageTypeName();

        internal Action<RabbitMqExchange, RabbitMappingContext> ConfigureSending = (e, c) =>
        {
            throw new NotImplementedException(); // TODO -- bindings should be off of the exchange

        };
    }
}
