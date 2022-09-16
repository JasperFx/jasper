using System;
using Jasper.Configuration;
using Jasper.ErrorHandling;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime.Interop.MassTransit;

namespace Jasper.RabbitMQ
{
    public class RabbitMqListenerConfiguration : ListenerConfiguration<RabbitMqListenerConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqListenerConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        ///     To optimize the message listener throughput,
        ///     start up multiple listening endpoints. This is
        ///     most necessary when using inline processing
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration ListenerCount(int count)
        {
            if (count <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count), "Must be greater than zero");
            }

            endpoint.ListenerCount = count;
            return this;
        }

        /// <summary>
        /// Add circuit breaker exception handling to this listener
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration CircuitBreaker(Action<CircuitBreakerOptions>? configure)
        {
            endpoint.CircuitBreakerOptions = new CircuitBreakerOptions();
            configure?.Invoke(endpoint.CircuitBreakerOptions);

            return this;
        }

        /// <summary>
        ///     Assume that any unidentified, incoming message types is the
        ///     type "T". This is primarily for interoperability with non-Jasper
        ///     applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqListenerConfiguration DefaultIncomingMessage<T>()
        {
            return DefaultIncomingMessage(typeof(T));
        }

        /// <summary>
        ///     Assume that any unidentified, incoming message types is the
        ///     type "T". This is primarily for interoperability with non-Jasper
        ///     applications
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public RabbitMqListenerConfiguration DefaultIncomingMessage(Type messageType)
        {
            endpoint.ReceivesMessage(messageType);
            return this;
        }

        /// <summary>
        /// Override the Rabbit MQ PreFetchCount value for just this endpoint for how many
        /// messages can be pre-fetched into memory before being handled
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration PreFetchCount(ushort count)
        {
            endpoint.PreFetchCount = count;
            return this;
        }

        /// <summary>
        /// Override the Rabbit MQ PreFetchSize value for just this endpoint for the total size of the
        /// messages that can be pre-fetched into memory before being handled
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration PreFetchSize(uint size)
        {
            endpoint.PreFetchSize = size;
            return this;
        }

        /// <summary>
        /// Add MassTransit interoperability to this Rabbit MQ listening endpoint
        /// </summary>
        /// <param name="configure">Optionally configure the JSON serialization on this endpoint</param>
        /// <returns></returns>
        public RabbitMqListenerConfiguration UseMassTransitInterop(Action<IMassTransitInterop>? configure = null)
        {
            endpoint.UseMassTransitInterop(configure);
            return this;
        }
    }
}
