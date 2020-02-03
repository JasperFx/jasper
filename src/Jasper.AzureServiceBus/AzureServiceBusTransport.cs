using System;
using System.Collections.Generic;
using System.Linq;
using Baseline;
using Jasper.AzureServiceBus.Internal;
using Jasper.Runtime;
using Jasper.Runtime.Routing;
using Jasper.Transports;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Primitives;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusTransport : TransportBase<AzureServiceBusEndpoint>, IAzureServiceBusTransport
    {
        public static readonly string ProtocolName = "asb";

        private readonly LightweightCache<Uri, AzureServiceBusEndpoint> _endpoints;

        public AzureServiceBusTopicRouter Topics { get; } = new AzureServiceBusTopicRouter();

        /// <summary>
        /// Azure Service Bus connection string as read from configuration
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// The Azure Service Bus RetryPolicy for this endpoint.
        /// </summary>
        public RetryPolicy RetryPolicy { get; set; } = Microsoft.Azure.ServiceBus.RetryPolicy.Default;

        /// <summary>
        /// Default is Amqp
        /// </summary>
        public TransportType TransportType { get; set; } = TransportType.Amqp;

        /// <summary>
        /// Set this for tokenized authentication
        /// </summary>
        public ITokenProvider TokenProvider { get; set; }

        /// <summary>
        /// Default is PeekLock
        /// </summary>
        public ReceiveMode ReceiveMode { get; set; } = ReceiveMode.PeekLock;

        public AzureServiceBusTransport() : base(ProtocolName)
        {
            _endpoints =
                new LightweightCache<Uri, AzureServiceBusEndpoint>(uri =>
                {
                    var endpoint = new AzureServiceBusEndpoint();
                    endpoint.Parse(uri);

                    endpoint.Parent = this;

                    return endpoint;
                });
        }

        protected override IEnumerable<AzureServiceBusEndpoint> endpoints()
        {
            return _endpoints;
        }

        protected override AzureServiceBusEndpoint findEndpointByUri(Uri uri)
        {
            return _endpoints[uri];
        }

        public AzureServiceBusEndpoint EndpointForQueue(string queueName)
        {
            var uri = new AzureServiceBusEndpoint{QueueName = queueName}.Uri;
            return _endpoints[uri];
        }

        public AzureServiceBusEndpoint EndpointForTopic(string topicName)
        {
            var uri = new AzureServiceBusEndpoint{TopicName = topicName}.Uri;
            return _endpoints[uri];
        }

    }
}
