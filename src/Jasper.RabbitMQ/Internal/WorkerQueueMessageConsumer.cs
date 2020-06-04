using System;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public abstract class MessageConsumerBase : DefaultBasicConsumer, IDisposable
    {
        protected readonly Uri _address;
        protected readonly IModel _channel;
        protected readonly ITransportLogger _logger;
        protected readonly IRabbitMqProtocol Mapper;
        private bool _latched;

        public MessageConsumerBase(ITransportLogger logger, IModel channel,
            IRabbitMqProtocol mapper, Uri address) : base(channel)
        {
            _logger = logger;
            _channel = channel;
            Mapper = mapper;
            _address = address;
        }

        public void Dispose()
        {
            _latched = true;
        }




        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
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
                envelope = Mapper.ReadEnvelope(body, properties);
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

            // THIS NEEDS TO BE VARIABLE
            executeEnvelope(deliveryTag, envelope);
        }

        protected abstract void executeEnvelope(ulong deliveryTag, Envelope envelope);
    }

    public class WorkerQueueMessageConsumer : MessageConsumerBase
    {
        private readonly IListeningWorkerQueue _callback;

        public WorkerQueueMessageConsumer(IListeningWorkerQueue callback, ITransportLogger logger, IModel channel,
            IRabbitMqProtocol mapper, Uri address) : base(logger, channel, mapper, address)
        {
            _callback = callback;
        }

        protected override void executeEnvelope(ulong deliveryTag, Envelope envelope)
        {
            _callback.Received(_address, envelope).ContinueWith(t =>
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
