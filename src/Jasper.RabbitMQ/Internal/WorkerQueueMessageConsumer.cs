using System;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class WorkerQueueMessageConsumer : DefaultBasicConsumer, IDisposable
    {
        private readonly Uri _address;
        private readonly RabbitMqSender _rabbitMqSender;
        private readonly RabbitMqListener Listener;
        private readonly IListeningWorkerQueue _callback;
        private readonly ITransportLogger _logger;
        private readonly IRabbitMqProtocol Mapper;
        private bool _latched;

        public WorkerQueueMessageConsumer(IListeningWorkerQueue callback, ITransportLogger logger, RabbitMqListener listener,
            IRabbitMqProtocol mapper, Uri address, RabbitMqSender rabbitMqSender)
        {
            _callback = callback;
            _logger = logger;
            Listener = listener;
            Mapper = mapper;
            _address = address;
            _rabbitMqSender = rabbitMqSender;
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
                Listener.Channel.BasicReject(deliveryTag, true);
                return;
            }

            var envelope = new RabbitMqEnvelope(Listener, deliveryTag);
            try
            {
                Mapper.ReadIntoEnvelope(envelope, properties, body);
            }
            catch (Exception e)
            {
                _logger.LogException(e, message: "Error trying to map an incoming RabbitMQ message to an Envelope");
                Model.BasicAck(envelope.DeliveryTag, false);

                return;
            }

            if (envelope.IsPing())
            {
                Model.BasicAck(deliveryTag, false);
                return;
            }

            _callback.Received(_address, envelope).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogException(t.Exception, envelope.Id, "Failure to receive an incoming message");
                    Model.BasicNack(deliveryTag, false, true);
                }
                // else
                // {
                //     Model.BasicAck(deliveryTag, false);
                // }
            });
        }
    }


}
