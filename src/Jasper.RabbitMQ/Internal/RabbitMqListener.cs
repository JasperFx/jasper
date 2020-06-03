using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqListener : RabbitMqConnectionAgent, IListener
    {
        private readonly ITransportLogger _logger;
        private readonly CancellationToken _cancellationToken;
        private readonly IRabbitMqProtocol _mapper;
        private MessageConsumer _consumer;
        private readonly string _routingKey;

        public RabbitMqListener(ITransportLogger logger, RabbitMqEndpoint endpoint, RabbitMqTransport transport, CancellationToken cancellationToken) : base(transport)
        {
            _logger = logger;
            _cancellationToken = cancellationToken;
            _mapper = endpoint.Protocol;
            Address = endpoint.Uri;

            _routingKey = endpoint.RoutingKey ?? endpoint.QueueName ?? "";
        }

        public ListeningStatus Status { get; set; } 


        private readonly BufferBlock<(Envelope Envelope, object AckObject)> _buffer = new BufferBlock<(Envelope Envelope, object AckObject)>(new DataflowBlockOptions
        {// DO NOT CHANGE THESE SETTINGS THEY ARE IMPORTANT TO LINK RECEIVE DELEGATE WITH CONSUME()
            BoundedCapacity = 1,
            MaxMessagesPerTask = 1,
            EnsureOrdered = true
        });
        public async IAsyncEnumerable<(Envelope Envelope, object AckObject)> Consume()
        {
            Start();

            while (!_cancellationToken.IsCancellationRequested)
            {
                yield return await _buffer.ReceiveAsync(_cancellationToken);
            }
        }

        public Task Ack((Envelope Envelope, object AckObject) messageInfo)
        {
            Channel.BasicAck((ulong)messageInfo.AckObject, false);
            return Task.CompletedTask;
        }

        public Task Nack((Envelope Envelope, object AckObject) messageInfo)
        {
            Channel.BasicNack((ulong)messageInfo.AckObject, false, true);
            return Task.CompletedTask;
        }

        public void Start()
        {
            EnsureConnected();

            Status = ListeningStatus.Accepting;

            _consumer = new MessageConsumer(_logger, Channel, _mapper, Address, _buffer, _cancellationToken)
            {
                ConsumerTag = Guid.NewGuid().ToString()
            };

            Channel.BasicConsume(_consumer, _routingKey);
        }

        public Uri Address { get; }

        public class MessageConsumer : DefaultBasicConsumer, IDisposable
        {
            private readonly Uri _address;
            private readonly BufferBlock<(Envelope Envelope, object AckObject)> _receiveBuffer;
            private readonly CancellationToken _cancellationToken;
            public readonly IModel _channel;
            private readonly ITransportLogger _logger;
            private readonly IRabbitMqProtocol _mapper;
            private bool _latched;

            public MessageConsumer(ITransportLogger logger, IModel channel,
                IRabbitMqProtocol mapper, Uri address, BufferBlock<(Envelope Envelope, object AckObject)> receiveBuffer, CancellationToken cancellationToken) : base(channel)
            {
                _logger = logger;
                _channel = channel;
                _mapper = mapper;
                _address = address;
                _receiveBuffer = receiveBuffer;
                _cancellationToken = cancellationToken;
            }

            public void Dispose()
            {
                _latched = true;
            }

            public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered,
                string exchange, string routingKey,
                IBasicProperties properties, byte[] body)
            {
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

                _receiveBuffer.SendAsync((envelope, deliveryTag), _cancellationToken).Wait(_cancellationToken);
            }
        }
    }
}
