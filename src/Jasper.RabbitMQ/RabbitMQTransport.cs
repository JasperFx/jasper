using System;
using System.IO;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Sending;
using Jasper.Messaging.Transports.Tcp;

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

    public class RabbitMQSender : ISender
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start(ISenderCallback callback)
        {
            throw new NotImplementedException();
        }

        public Task Enqueue(Envelope envelope)
        {
            throw new NotImplementedException();
        }

        public Uri Destination { get; }
        public int QueuedCount { get; }
        public bool Latched { get; }
        public Task LatchAndDrain()
        {
            throw new NotImplementedException();
        }

        public void Unlatch()
        {
            throw new NotImplementedException();
        }

        public Task Ping()
        {
            throw new NotImplementedException();
        }
    }

    public class RabbitMQListeningAgent : IListener
    {
        public Task<ReceivedStatus> Received(Uri uri, Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public Task Acknowledged(Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public Task NotAcknowledged(Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public Task Failed(Exception exception, Envelope[] messages)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public Uri Address { get; }
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
