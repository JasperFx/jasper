using System;
using Baseline;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly RabbitMqTransport _transport;
        private readonly ITransportLogger _logger;
        private readonly IRabbitMqProtocol _mapper;
        private IReceiverCallback _callback;
        private MessageConsumer _consumer;
        private string _exchangeName;
        private string _routingKey;

        public RabbitMqListener(ITransportLogger logger,
            RabbitMqEndpoint endpoint, RabbitMqTransport transport) : base(transport)
        {
            _logger = logger;
            _mapper = endpoint.Protocol;
            _transport = transport;
            Address = endpoint.Uri;

            _exchangeName = endpoint.ExchangeName ?? "";
            _routingKey = endpoint.RoutingKey ?? endpoint.QueueName ?? "";
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

                        Channel.BasicCancel(_consumer.ConsumerTag);
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

            Connect();

            _callback = callback;
            _consumer = new MessageConsumer(callback, _logger, Channel, _mapper, Address)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            Channel.BasicConsume(_consumer, _routingKey);
        }


        public Uri Address { get; }

        public class MessageConsumer : DefaultBasicConsumer, IDisposable
        {
            private readonly Uri _address;
            private readonly IReceiverCallback _callback;
            private readonly IModel _channel;
            private readonly ITransportLogger _logger;
            private readonly IRabbitMqProtocol _mapper;
            private bool _latched;

            public MessageConsumer(IReceiverCallback callback, ITransportLogger logger, IModel channel,
                IRabbitMqProtocol mapper, Uri address) : base(channel)
            {
                _callback = callback;
                _logger = logger;
                _channel = channel;
                _mapper = mapper;
                _address = address;
            }

            public void Dispose()
            {
                _latched = true;
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
                string exchange, string routingKey,
                IBasicProperties properties, byte[] body)
            {
                if (_callback == null) return;

                if (_latched)
                {
                    _channel.BasicReject(deliveryTag, true);
                    return;
                }

                Envelope envelope;
                try
                {
                    envelope = _mapper.ReadEnvelope(body, properties);
                }
                catch (Exception e)
                {
                    _logger.LogException(e, message: "Error trying to map an incoming RabbitMQ message to an Envelope");
                    _channel.BasicAck(deliveryTag, false);

                    return;
                }

                if (envelope.IsPing())
                {
                    _channel.BasicAck(deliveryTag, false);
                    return;
                }

                _callback.Received(_address, new[] {envelope}).ContinueWith(t =>
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
        }
    }
}
