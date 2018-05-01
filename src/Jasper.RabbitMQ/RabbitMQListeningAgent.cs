using System;
using System.Threading.Tasks;
using Jasper.Messaging.Logging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Receiving;
using Jasper.Messaging.Transports.Tcp;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{


    public class RabbitMQListeningAgent : IListeningAgent
    {
        private readonly ITransportLogger _logger;
        private readonly IModel _channel;
        private readonly IEnvelopeMapper _mapper;
        private readonly string _queue;
        private MessageConsumer _consumer;
        private IReceiverCallback _callback;

        public RabbitMQListeningAgent(Uri address, ITransportLogger logger, IModel channel, IEnvelopeMapper mapper, RabbitMqAgent agent)
        {
            _logger = logger;
            _channel = channel;
            _mapper = mapper;
            Address = address;
            _queue = agent.QueueName;
        }

        public void Dispose()
        {
            _consumer.Dispose();
        }

        public ListeningStatus Status
        {
            get => _consumer != null ? ListeningStatus.Accepting : ListeningStatus.TooBusy;
            set
            {
                switch (value)
                {
                    case ListeningStatus.TooBusy when _consumer != null:
                        _consumer.Dispose();
                        _channel.BasicCancel(_consumer.ConsumerTag);
                        _consumer = null;
                        break;
                    case ListeningStatus.Accepting when _consumer == null:
                        Start(_callback);
                        break;
                }
            }
        }

        public void Start(IReceiverCallback callback)
        {
            if (callback == null) return;

            _callback = callback;
            _consumer = new MessageConsumer(callback, _logger, _channel, _mapper, Address)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            _channel.BasicConsume(_consumer, _queue, autoAck: false);
        }

        public class MessageConsumer : DefaultBasicConsumer, IDisposable
        {
            private readonly IReceiverCallback _callback;
            private readonly ITransportLogger _logger;
            private readonly IModel _channel;
            private readonly IEnvelopeMapper _mapper;
            private readonly Uri _address;
            private bool _latched;

            public MessageConsumer(IReceiverCallback callback, ITransportLogger logger, IModel channel,
                IEnvelopeMapper mapper, Uri address) : base(channel)
            {
                _callback = callback;
                _logger = logger;
                _channel = channel;
                _mapper = mapper;
                _address = address;
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
                IBasicProperties properties, byte[] body)
            {
                if (_callback == null) return;

                if (_latched)
                {
                    _channel.BasicReject(deliveryTag, true);
                    return;
                }

                Envelope envelope = null;
                try
                {
                    envelope = _mapper.ReadEnvelope(body, properties);
                }
                catch (Exception e)
                {
                    _logger.LogException(e, message:"Error trying to map an incoming RabbitMQ message to an Envelope");
                    _channel.BasicAck(deliveryTag, false);

                    return;
                }

                _callback.Received(_address, new [] {envelope}).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        _logger.LogException(t.Exception, envelope.Id, "Failure to receive an incoming message");
                        _channel.BasicNack(deliveryTag, false, true);
                    }
                    else
                    {
                        _channel.BasicAck(deliveryTag, false);
                    }
                });

            }

            public void Dispose()
            {
                _latched = true;
            }
        }



        public Uri Address { get; }
    }
}
