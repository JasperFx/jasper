using System;
using Baseline;
using DotPulsar.Abstractions;
using Jasper.Configuration;
using Jasper.Runtime;
using Microsoft.Extensions.Hosting;

namespace Jasper.Pulsar
{
    public static class PulsarTransportExtensions
    {
        /// <summary>
        /// Retrieve the PulsarTransport object from within the running
        /// host
        /// </summary>
        /// <param name="host"></param>
        /// <returns></returns>
        private static PulsarTransport PulsarTransport(this IHost host)
        {
            return host
                .Get<IMessagingRoot>()
                .Options
                .Endpoints
                .As<TransportCollection>()
                .Get<PulsarTransport>();
        }

        /// <summary>
        /// Quick access to the Pulsar Transport within this application.
        /// This is for advanced usage
        /// </summary>
        /// <param name="endpoints"></param>
        /// <returns></returns>
        internal static PulsarTransport PulsarTransport(this IEndpoints endpoints)
        {
            var transports = endpoints.As<TransportCollection>();
            var transport = transports.Get<PulsarTransport>();
            if (transport == null)
            {
                transport = new PulsarTransport();
                transports.Add(transport);
            }

            return transport;
        }

        /// <summary>
        /// Configure connection and authentication information about the Pulsar usage
        /// within this Jasper application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="configure"></param>
        public static void ConfigurePulsar(this IEndpoints endpoints, Action<IPulsarClientBuilder> configure)
        {
            configure(endpoints.PulsarTransport().Builder);
        }

        /// <summary>
        /// Connect to a local, standalone Pulsar broker at the default port
        /// </summary>
        /// <param name="endpoints"></param>
        public static void ConnectToLocalPulsar(this IEndpoints endpoints)
        {
            endpoints.ConfigurePulsar(x => {});
        }

        /// <summary>
        /// Publish matching messages to Pulsar using the named routing key or queue name and
        /// optionally an exchange
        /// </summary>
        /// <param name="publishing"></param>
        /// <param name="topicPath">Pulsar topic of the form "persistent|non-persistent://tenant/namespace/topic"</param>
        /// <returns></returns>
        public static PulsarSubscriberConfiguration ToPulsar(this IPublishToExpression publishing, string topicPath)
        {
            var transports = publishing.As<PublishingExpression>().Parent;
            var transport = transports.Get<PulsarTransport>();
            var endpoint = transport.EndpointFor(topicPath);

            // This is necessary unfortunately to hook up the subscription rules
            publishing.To(endpoint.Uri);

            return new PulsarSubscriberConfiguration(endpoint);
        }


        /// <summary>
        /// Listen to a specified Pulsar topic path of the path "persistent|non-persistent://tenant/namespace/topic"
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="topicPath"></param>
        /// <returns></returns>
        public static PulsarListenerConfiguration ListenToPulsarTopic(this IEndpoints endpoints, string topicPath)
        {
            var uri = PulsarEndpoint.UriFor(topicPath);
            var endpoint = endpoints.PulsarTransport()[uri];
            endpoint.IsListener = true;
            return new PulsarListenerConfiguration(endpoint);
        }
    }



    public class PulsarListenerConfiguration : ListenerConfiguration<PulsarListenerConfiguration, PulsarEndpoint>
    {
        public PulsarListenerConfiguration(PulsarEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PulsarListenerConfiguration Protocol<T>() where T : IPulsarProtocol, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public PulsarListenerConfiguration Protocol(IPulsarProtocol protocol)
        {
            endpoint.Protocol = protocol;
            return this;
        }

        // /// <summary>
        // /// To optimize the message listener throughput,
        // /// start up multiple listening endpoints. This is
        // /// most necessary when using inline processing
        // /// </summary>
        // /// <param name="count"></param>
        // /// <returns></returns>
        // public PulsarListenerConfiguration ListenerCount(int count)
        // {
        //     if (count <= 0) throw new ArgumentOutOfRangeException(nameof(count), "Must be greater than zero");
        //
        //     endpoint.ListenerCount = count;
        //     return this;
        // }
    }

    public class PulsarSubscriberConfiguration : SubscriberConfiguration<PulsarSubscriberConfiguration, PulsarEndpoint>
    {
        public PulsarSubscriberConfiguration(PulsarEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public PulsarSubscriberConfiguration Protocol<T>() where T : IPulsarProtocol, new()
        {
            return Protocol(new T());
        }

        /// <summary>
        /// Override the header protocol for outgoing messages at this location. This is mostly
        /// useful for integrating Jasper with non-Jasper applications
        /// </summary>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public PulsarSubscriberConfiguration Protocol(IPulsarProtocol protocol)
        {
            _endpoint.Protocol = protocol;
            return this;
        }


    }
}
