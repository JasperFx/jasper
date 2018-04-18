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
        private readonly RabbitMQSettings _settings;

        public RabbitMQTransport(RabbitMQSettings rabbitMqSettings, IDurableMessagingFactory factory, ITransportLogger logger, MessagingSettings settings)
            : base("rabbitmq", factory, logger, settings)
        {
            _settings = rabbitMqSettings;
        }

        protected override ISender createSender(Uri uri, CancellationToken cancellation)
        {
            var agent = _settings.For(uri);
            return agent.CreateSender(logger, cancellation);
        }

        protected override Uri[] validateAndChooseReplyChannel(Uri[] incoming)
        {
            return incoming;
        }

        protected override IListeningAgent buildListeningAgent(Uri uri, MessagingSettings settings)
        {
            var agent = _settings.For(uri);
            return agent.CreateListeningAgent(uri, settings, logger);
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
