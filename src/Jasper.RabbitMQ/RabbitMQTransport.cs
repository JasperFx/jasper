using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.RabbitMQ
{
    public class RabbitMQTransport : ITransport
    {
        private readonly RabbitMqSettings _settings;

        public RabbitMQTransport(RabbitMqSettings settings)
        {
            _settings = settings;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Protocol { get; } = "rabbitmq";

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Uri LocalReplyUri { get; }

        public void StartListening(IMessagingRoot root)
        {
            throw new NotImplementedException();
        }

        public void Describe(TextWriter writer)
        {
            throw new NotImplementedException();
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
