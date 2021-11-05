using System;
using System.Threading;
using Jasper.Logging;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class WorkerQueueMessageConsumer : DefaultBasicConsumer, IDisposable
    {
        private readonly Uri _address;
        private readonly RabbitMqSender _rabbitMqSender;
        private readonly CancellationToken _cancellation;
        private readonly RabbitMqListener Listener;
        private readonly IListeningWorkerQueue _workerQueue;
        private readonly ITransportLogger _logger;
        private readonly IRabbitMqProtocol Mapper;
        private bool _latched;

        public WorkerQueueMessageConsumer(IListeningWorkerQueue workerQueue, ITransportLogger logger,
            RabbitMqListener listener,
            IRabbitMqProtocol mapper, Uri address, RabbitMqSender rabbitMqSender, CancellationToken cancellation)
        {
            _workerQueue = workerQueue;
            _logger = logger;
            Listener = listener;
            Mapper = mapper;
            _address = address;
            _rabbitMqSender = rabbitMqSender;
            _cancellation = cancellation;
        }

        public void Dispose()
        {
            _latched = true;
        }

        public override void HandleBasicDeliver(string consumerTag, ulong deliveryTag, bool redelivered, string exchange, string routingKey,
            IBasicProperties properties, byte[] body)
        {
            if (_latched || _cancellation.IsCancellationRequested)
            {
                Listener.Channel.BasicReject(deliveryTag, true);
                return;
            }

            var envelope = new RabbitMqEnvelope(Listener, deliveryTag);
            try
            {
                envelope.Data = body;
                Mapper.MapIncomingToEnvelope(envelope, properties);
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

            _workerQueue.Received(_address, envelope).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogException(t.Exception, envelope.Id, "Failure to receive an incoming message");
                    Model.BasicNack(deliveryTag, false, true);
                }
            });
        }
    }


}
