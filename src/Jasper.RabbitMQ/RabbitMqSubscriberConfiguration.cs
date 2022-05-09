using System;
using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;

namespace Jasper.RabbitMQ
{
    public class
        RabbitMqSubscriberConfiguration : SubscriberConfiguration<RabbitMqSubscriberConfiguration, RabbitMqEndpoint>
    {
        public RabbitMqSubscriberConfiguration(RabbitMqEndpoint endpoint) : base(endpoint)
        {
        }

        /// <summary>
        ///     Configure raw properties of this RabbitMqEndpoint. Advanced usages
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public RabbitMqSubscriberConfiguration Advanced(Action<RabbitMqEndpoint> configure)
        {
            configure(_endpoint);
            return this;
        }
    }
}
