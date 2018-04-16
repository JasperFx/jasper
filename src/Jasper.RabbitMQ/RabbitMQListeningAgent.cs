using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class RabbitMQListeningAgent : IListeningAgent
    {
        private readonly IModel _channel;
        private readonly IEnvelopeMapper _mapper;
        private EventingBasicConsumer _consumer;

        public RabbitMQListeningAgent(Uri address, IModel channel, IEnvelopeMapper mapper)
        {
            _channel = channel;
            _mapper = mapper;
            Address = address;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Start(IReceiverCallback callback)
        {
            _consumer = new EventingBasicConsumer(_channel);
            _consumer.Received += (sender, args) =>
            {
                var envelope = _mapper.ReadEnvelope(args);

            };
        }

        public Uri Address { get; }
    }
}
