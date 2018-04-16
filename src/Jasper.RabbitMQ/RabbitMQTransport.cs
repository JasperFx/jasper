using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using Jasper.Messaging;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Configuration;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.RabbitMQ
{
    public class RabbitMQTransport : TransportBase
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMQTransport(RabbitMqSettings rabbitMqSettings, IPersistence persistence, ITransportLogger logger, MessagingSettings settings)
            : base("rabbitmq", persistence, logger, settings)
        {
            _settings = rabbitMqSettings;
        }

        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            var agent = _settings.For(uri);
            return agent.CreateSender(cancellation);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            throw new NotImplementedException();
        }

        protected override IListeningAgent buildListeningAgent(Uri uri, MessagingSettings settings)
        {
            var agent = _settings.For(uri);
            return agent.CreateListeningAgent(uri, settings);
        }
    }

    /*
     * Notes
     * Will need to be able to configure the RabbitMQ ConnectionFactory
     * Send the envelope through as a byte array in the body
     * All Agents will need a
     * Use the ServiceName.ToLowerCase-replies as the reply queue
     * Need to use the basic ack to finish the receiving
     */
}
